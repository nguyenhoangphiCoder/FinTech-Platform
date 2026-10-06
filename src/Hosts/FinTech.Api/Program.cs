var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Minimal API Base Endpoints
app.MapGet("/", () => Results.Ok(new
{
    Service = "FinTech Platform Core API",
    Version = "10.0.0-preview",
    Status = "Healthy",
    Timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/health", () => Results.Ok(new { Status = "UP" }));

app.Run();
