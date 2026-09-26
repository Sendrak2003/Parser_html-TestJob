using System.Text.Encodings.Web;
using FluentValidation;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.WriteIndented = true;
        o.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });
builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<ElementsRequestValidator>();
builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(
    builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.")));
builder.Services.AddSingleton<ParserService>();

var app = builder.Build();

await app.Services.GetRequiredService<ParserService>().InitializeAsync();

app.MapOpenApi();
app.UseSwaggerUI(o =>
{
    o.RoutePrefix = "api/swagger";
    o.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.UseAuthorization();

app.MapControllers();

app.Run();
