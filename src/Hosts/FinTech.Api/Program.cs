using FinTech.Modules.Identity;
using FinTech.Modules.Ledger;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProblemDetails();

// Cấu hình chuẩn OpenAPI 3.1 tích hợp của .NET 10
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "FinTech Platform Core API";
        document.Info.Version = "v1";
        document.Info.Description = "Enterprise FinTech Platform REST API (.NET 10 LTS, Clean Architecture, SQL Server 2025)";
        return Task.CompletedTask;
    });
});

// Register Modules
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddLedgerModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

// Map Module Endpoints
app.MapIdentityEndpoints();
app.MapLedgerEndpoints();

// Kích hoạt OpenAPI & Scalar API Reference Documentation (giao diện thế hệ mới thay thế Swagger)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("FinTech Platform API Reference")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Redirect trang chủ về trang tài liệu API
app.MapGet("/", () => Results.Redirect("/scalar/v1"))
   .ExcludeFromDescription();

// Minimal API Base Endpoints
app.MapGet("/api/health", () => Results.Ok(new
{
    Service = "FinTech Platform Core API",
    Version = "10.0.0-preview",
    Status = "Healthy",
    Documentation = "/scalar/v1",
    Timestamp = DateTimeOffset.UtcNow
}))
.WithName("GetHealthStatus")
.WithTags("Health & System")
.Produces(StatusCodes.Status200OK);

app.Run();
