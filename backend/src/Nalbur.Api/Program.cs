using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nalbur.Api.Endpoints;
using Nalbur.Api.ErrorHandling;
using Nalbur.Application;
using Nalbur.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration: everything comes from environment variables ----
// ASP.NET Core loads environment variables into IConfiguration automatically.
var shopName = builder.Configuration["SHOP_NAME"] ?? "Nalbur Stok";
var defaultCritical = builder.Configuration.GetValue("CRITICAL_STOCK_DEFAULT", 5m);
var seedSampleData = builder.Configuration.GetValue("SEED_SAMPLE_DATA", false);
var allowedOrigins = (builder.Configuration["ALLOWED_ORIGINS"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// ConnectionStrings__Nalbur in the environment. No default: the password must not live in the image.
var connectionString = builder.Configuration.GetConnectionString("Nalbur")
    ?? throw new InvalidOperationException("ConnectionStrings__Nalbur must be set.");

if (defaultCritical < 0)
{
    // Fail fast at startup instead of misbehaving later.
    throw new InvalidOperationException("CRITICAL_STOCK_DEFAULT must be zero or greater.");
}

// ---- Services: the onion layers are wired together only here ----
builder.Services.AddApplication(new StockSettings(defaultCritical));
builder.Services.AddInfrastructure(connectionString);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI document at /openapi/v1.json, rendered by Scalar at /scalar
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// CORS is only for local development (Vite on :5173 calling the API on :5080).
// In Docker, nginx proxies /api, so the browser sees one origin and CORS is unused.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (allowedOrigins.Length > 0)
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    }
}));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

// ---- API docs ----
// Enabled in every environment: the API holds no secrets and the docs are useful in Docker too.
app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle($"{shopName} API"));

// ---- Endpoints ----
// 200 when the API and its database are up, 503 otherwise. Docker HEALTHCHECK uses this.
// MapGet instead of MapHealthChecks so the endpoint also shows up in the OpenAPI document.
app.MapGet("/health", async (HealthCheckService health, CancellationToken ct) =>
{
    var report = await health.CheckHealthAsync(ct);
    var body = new
    {
        status = report.Status.ToString().ToLowerInvariant(),
        checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString().ToLowerInvariant()),
    };
    return report.Status == HealthStatus.Healthy
        ? Results.Ok(body)
        : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/api/info", () => Results.Ok(new
{
    shopName,
    defaultCriticalLevel = defaultCritical,
    environment = app.Environment.EnvironmentName,
    version = typeof(Program).Assembly.GetName().Version?.ToString(3),
}));

app.MapProductEndpoints();
app.MapStockEndpoints();

// ---- Database: apply migrations, then optional sample data ----
await app.Services.InitializeDatabaseAsync(seedSampleData);

app.Run();
