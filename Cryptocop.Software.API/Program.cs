using System.Text.Json.Serialization;
using Cryptocop.Software.API.Middleware;
using Cryptocop.Software.API.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Cryptocop.Software.API.Repositories.Data;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Repositories.Implementations;
using Microsoft.OpenApi.Models;
using Cryptocop.Software.API.Services.Interfaces;
using Cryptocop.Software.API.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddHttpClient<IShoppingCartService, ShoppingCartService>(client =>
{
    var baseUrl = builder.Configuration["Messari:BaseUrl"] ?? "https://data.messari.io";
    client.BaseAddress = new Uri(baseUrl);
    var apiKey = builder.Configuration["Messari:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("x-messari-api-key", apiKey);
    }
});
builder.Services.AddHttpClient<ICryptoCurrencyService, CryptoCurrencyService>(client =>
{
    var baseUrl = builder.Configuration["Messari:BaseUrl"] ?? "https://data.messari.io";
    client.BaseAddress = new Uri(baseUrl);
    var apiKey = builder.Configuration["Messari:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("x-messari-api-key", apiKey);
    }
});
builder.Services.AddHttpClient<IExchangeService, ExchangeService>(client =>
{
    var baseUrl = builder.Configuration["Messari:BaseUrl"] ?? "https://data.messari.io";
    client.BaseAddress = new Uri(baseUrl);
    var apiKey = builder.Configuration["Messari:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("x-messari-api-key", apiKey);
    }
});

// Database
var dbConnection = builder.Configuration.GetConnectionString("CryptocopDb");
if (!string.IsNullOrWhiteSpace(dbConnection))
{
    builder.Services.AddDbContext<CryptocopDbContext>(options =>
        options.UseNpgsql(dbConnection));
}

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cryptocop API", Version = "v1" });

    // Enable JWT bearer in Swagger UI (Authorize button)
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// Dev CORS for local React app
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://web:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT authentication + global authorization policy (all endpoints require auth by default)
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply pending EF Core migrations on startup (safe no-op if up-to-date)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetService<Cryptocop.Software.API.Repositories.Data.CryptocopDbContext>();
    db?.Database.Migrate();
}
catch (Exception ex)
{
    Console.WriteLine($"Database migration failed: {ex.Message}");
}

// In development, avoid redirecting HTTP->HTTPS to keep Swagger working without trusting certs.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}
// Map exceptions to ProblemDetails consistently
app.UseMiddleware<ProblemDetailsExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
