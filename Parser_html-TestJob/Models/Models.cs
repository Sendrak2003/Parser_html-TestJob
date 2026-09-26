using System.Text.Json.Serialization;

public sealed record Payload ([property: JsonPropertyName("selector")] string? Selector = null,
                       [property: JsonPropertyName("attribute")] string? Attribute = null,
                       [property: JsonPropertyName("url_b64")] string? UrlB64 = null,
                       [property: JsonPropertyName("encrypted_text_bytes_b64")] string? EncryptedTextBytesB64 = null,
                       [property: JsonPropertyName("key_bytes_b64")] string? KeyBytesB64 = null,
                       [property: JsonPropertyName("page_b64")] string? PageB64 = null);

public sealed record Result
{
    [JsonPropertyName("is_error")] public int IsError { get; init; }
    [JsonPropertyName("error_code")] public ErrorType ErrorCode { get; init; } = ErrorType.None;
    [JsonPropertyName("error_message")] public string? ErrorMessage { get; init; }
    [JsonPropertyName("elements_count")] public int ElementsCount { get; init; }
    [JsonPropertyName("emails_count")] public int EmailsCount { get; init; }
    [JsonPropertyName("url")] public string? Url { get; init; }
    [JsonPropertyName("decrypted_plain_text")] public string? DecryptedPlainText { get; init; }
    [JsonPropertyName("elements_attr_list")] public List<string?> ElementsAttrList { get; init; } = [];
    [JsonPropertyName("emails_list")] public List<string> EmailsList { get; init; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    None = 0,
    MissingParameter = 1,
    EmptySelector = 2,
    EmptyAttribute = 3,
    InvalidSelector = 4,
    InvalidUrlBase64 = 5,
    InvalidPageBase64 = 6,
    InvalidEncryptedTextBytes = 7,
    InvalidKeyBytes = 8,
    ParsingFailed = 9,
    DecryptionFailed = 10,
    DatabaseError = 11,
    UnknownError = 99
}

public class CustomError : Exception
{
    public ErrorType Code { get; }

    public CustomError(ErrorType code, string message) : base(message)
    {
        Code = code;
    }
}
