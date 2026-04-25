using System.Text;
using Caesura.Api.Infrastructure.OpenApi;
using Caesura.Api.Middleware;
using Caesura.Api.Validators.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default");
var jwtSecret = builder.Configuration["Jwt:Secret"] ??
                throw new InvalidOperationException("Jwt:Secret is not configured");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    var origins = builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:3000"];

    policy.WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
}));

// ── HTTP JSON options (used by OpenAPI schema generator) ─────────────────────
// Must mirror the MVC JsonOptions so the generated schema reflects snake_case
// field names rather than the C# PascalCase property names.
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
    opts.SerializerOptions.PropertyNameCaseInsensitive = true;
    opts.SerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    opts.SerializerOptions.Encoder =
        System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<BooksService>();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// Suppress the built-in model state filter; FluentValidation handles everything.
// IMPORTANT: We still handle null bodies explicitly in ValidateAsync<T>.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;

    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => SnakeCaseNamingPolicy.Instance.ConvertName(kvp.Key),
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        if (errors.Count == 0)
        {
            return new BadRequestObjectResult(new
            {
                error = "Request body is missing or malformed. Ensure Content-Type is application/json."
            });
        }

        return new UnprocessableEntityObjectResult(new
        {
            error = "Validation failed.",
            errors
        });
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        opts.JsonSerializerOptions.Encoder =
            System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// ── OpenAPI (native .NET 10, served at /openapi/v1.json) ─────────────────────
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;

    // Document-level metadata
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Caesura API",
            Version = "v1",
            Description = "REST API for the Caesura book-reading platform. " +
                          "Authenticate with a JWT Bearer token to access protected endpoints.",
            Contact = new OpenApiContact
            {
                Name = "Caesura",
                Url = new Uri("https://caesura.app")
            },
            License = new OpenApiLicense
            {
                Name = "MIT"
            }
        };
        return Task.CompletedTask;
    });

    // Register the JWT Bearer security scheme on the document
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

    // Attach the security requirement to every [Authorize] operation
    options.AddOperationTransformer<AuthorizeOperationTransformer>();
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Serve the raw OpenAPI JSON at /openapi/v1.json
    app.MapOpenApi();

    // Scalar interactive UI at /scalar
    app.MapScalarApiReference(options =>
    {
        options.Title = "Caesura API";
        options.Theme = ScalarTheme.Default;
        options.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = ["Bearer"]
        };
    });
}

app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();