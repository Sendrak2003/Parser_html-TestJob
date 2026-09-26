using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Dapper;
using Npgsql;

public class ParserService
{
    private static readonly Regex EmailRegex = new(
        @"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly NpgsqlDataSource _dataSource;

    public ParserService(NpgsqlDataSource dataSource)
        => _dataSource = dataSource;

    public async Task InitializeAsync()
    {
        try
        {
            await InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            throw new CustomError(ErrorType.DatabaseError, $"Database connection failed: {ex.Message}");
        }
    }

    public async Task<Result> ProcessAsync(Payload request)
    {
        var url = Encoding.UTF8.GetString(DecodeBase64(request.UrlB64!, ErrorType.InvalidUrlBase64, "url_b64"));
        var pageHtml = Encoding.UTF8.GetString(DecodeBase64(request.PageB64!, ErrorType.InvalidPageBase64, "page_b64"));

        using var context = BrowsingContext.New(Configuration.Default);
        using var document = await context.OpenAsync(req => req.Content(pageHtml));

        List<IElement> elements;
        try
        {
            elements = document.QuerySelectorAll(request.Selector!).ToList();
        }
        catch (DomException ex)
        {
            throw new CustomError(ErrorType.InvalidSelector, $"Invalid CSS selector: {ex.Message}");
        }
        var attrValues = elements.Select(e => e.GetAttribute(request.Attribute!)).ToList();

        var emails = EmailRegex.Matches(pageHtml).Select(m => m.Value).Distinct().ToList();

        var cipher = DecodeBase64(request.EncryptedTextBytesB64!, ErrorType.InvalidEncryptedTextBytes, "encrypted_text_bytes_b64");
        var key = DecodeBase64(request.KeyBytesB64!, ErrorType.InvalidKeyBytes, "key_bytes_b64");
        if (key.Length != 32)
            throw new CustomError(ErrorType.InvalidKeyBytes, $"AES-256 key must be 32 bytes, got {key.Length}.");

        string decryptedText;
        try
        {
            using var aes = Aes.Create();
            aes.Key = key;
            decryptedText = Encoding.UTF8.GetString(aes.DecryptEcb(cipher, PaddingMode.None));
        }
        catch (CryptographicException ex)
        {
            throw new CustomError(ErrorType.DecryptionFailed, $"Decryption failed: {ex.Message}");
        }

        var rows = elements
            .Select((e, i) => new { AttributeValue = attrValues[i], ElementHtml = e.OuterHtml })
            .ToList();
        await InsertElementsAsync(rows);

        return new Result
        {
            ElementsCount = elements.Count,
            EmailsCount = emails.Count,
            Url = url,
            DecryptedPlainText = decryptedText,
            ElementsAttrList = attrValues,
            EmailsList = emails
        };
    }

    private static byte[] DecodeBase64(string value, ErrorType error, string name)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException ex)
        {
            throw new CustomError(error, $"Invalid base64 in '{name}': {ex.Message}");
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS elements (
                id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                attribute_value TEXT,
                element_html TEXT NOT NULL
            )
            """);
    }

    private async Task InsertElementsAsync(IReadOnlyCollection<object> rows)
    {
        if (rows.Count == 0)
            return;

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync("""
                INSERT INTO elements (attribute_value, element_html)
                VALUES (@AttributeValue, @ElementHtml)
                """, rows, transaction);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            throw new CustomError(ErrorType.DatabaseError, $"Failed to save elements: {ex.Message}");
        }
    }
}
