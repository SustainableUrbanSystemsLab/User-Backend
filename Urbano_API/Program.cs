using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Urbano_API.Models;
using Urbano_API.Services;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;
using System.Security.Claims;
using Microsoft.IdentityModel.Logging;
using System.Text.Json.Serialization;

IdentityModelEventSource.ShowPII = true; // Enable detailed JWT errors

var builder = WebApplication.CreateBuilder(args);

// =============================================
// Enhanced Configuration Setup
// =============================================
if (builder.Environment.IsDevelopment())
{
    // Development: Use .env + User Secrets
    DotNetEnv.Env.Load();
    builder.Configuration.AddUserSecrets<Program>();
    Console.WriteLine("Running in DEVELOPMENT mode (using .env + user secrets)");
}
else
{
    // Production: Use Render environment variables
    builder.Configuration.AddEnvironmentVariables();
    Console.WriteLine("Running in PRODUCTION mode (using Render env vars)");
}

// =============================================
// Environment-Safe Configuration Access
// =============================================
var GetConfigValue = new Func<string, string?>((key) => 
{
    // Try config first, then fallback to environment variables
    var value = builder.Configuration[key] ?? Environment.GetEnvironmentVariable(key);
    
    if (string.IsNullOrEmpty(value) && !key.Contains("Password") && !key.Contains("Secret"))
    {
        Console.WriteLine($"Configuration Warning: {key} is null or empty");
    }
    
    return value;
});

// Get all configuration values
var smtpUsername = GetConfigValue("Mailing:SmtpUsername") ?? GetConfigValue("SMTP_USERNAME");
var smtpPassword = GetConfigValue("Mailing:SmtpPassword") ?? GetConfigValue("SMTP_PASSWORD");
var smtpServer = GetConfigValue("Mailing:SmtpServer") ?? GetConfigValue("SMTP_SERVER");
var smtpPort = GetConfigValue("Mailing:SmtpPort") ?? GetConfigValue("SMTP_PORT");
var mailingEmailFrom = GetConfigValue("Mailing:FromEmail") ?? GetConfigValue("MAILING_EMAILFROM");
var mailingEmailName = GetConfigValue("Mailing:FromName") ?? GetConfigValue("MAILING_EMAILNAME");
var mongoConnectionString = GetConfigValue("UrbanoDatabase:ConnectionString") ?? GetConfigValue("MONGO_CONNECTIONSTRING");
var jwtKey = GetConfigValue("SecretKey") ?? GetConfigValue("SECRETKEY");
var salt = GetConfigValue("passwordSalt") ?? GetConfigValue("PASSWORDSALT");

// =============================================
// Debug Output
// =============================================
Console.WriteLine($"Key: '{jwtKey?.Substring(0, Math.Min(5, jwtKey?.Length ?? 0))}...'");
Console.WriteLine($"Length: {jwtKey?.Length ?? 0} chars");
if (jwtKey != null)
{
    Console.WriteLine($"Byte Size: {Encoding.UTF8.GetBytes(jwtKey).Length} bytes");
    Console.WriteLine($"Bit Size: {Encoding.UTF8.GetBytes(jwtKey).Length * 8} bits");
}

// =============================================
// Configuration Validation
// =============================================
if (string.IsNullOrEmpty(mongoConnectionString))
{
    Console.WriteLine("CRITICAL ERROR: MongoDB Connection String is missing!");
    if (builder.Environment.IsProduction())
    {
        throw new Exception("MongoDB connection string is required in production");
    }
}

// CORS setup
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "whitelistedURLs",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173",
                                              "https://localhost:5173",
                                              "https://urbano-frontend.onrender.com",
                                              "https://urbano-backend.onrender.com",
                                              "https://hoppscotch.io")
                                                   .AllowAnyHeader()
                                                  .AllowAnyMethod();
                      });
});

// Service configuration
builder.Services.Configure<UrbanoStoreDatabaseSettings>(
    builder.Configuration.GetSection("UrbanoDatabase"));

builder.Services.Configure<UrbanoStoreEmailSettings>(
    builder.Configuration.GetSection("Mailing"));

// Service registrations
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IVerificationService, VerificationService>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IVerificationRepository, VerificationRepository>();
builder.Services.AddSingleton<IMetricsRepository, MetricsRepository>();
builder.Services.AddSingleton<IRegistrationsRepository, RegistrationsRepository>();
builder.Services.AddSingleton<ISimulationsRepository, SimulationsRepository>();
builder.Services.AddSingleton<IWalletRepository, WalletRepository>();
builder.Services.AddSingleton<ILoginsRepository, LoginsRepository>();

// JWT Authentication setup
if (!string.IsNullOrEmpty(jwtKey))
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey.Trim()));
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var claimsIdentity = context!.Principal!.Identity as ClaimsIdentity;
                var roleClaim = claimsIdentity!.FindFirst(ClaimTypes.Role)?.Value;
                var emailClaim = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var errorResponse = new { success = false, message = "Authentication failed. Invalid token or not authenticated." };
                return context.Response.WriteAsJsonAsync(errorResponse);
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var errorResponse = new { success = false, message = "Access Denied: You do not have permission to access this resource." };
                return context.Response.WriteAsJsonAsync(errorResponse);
            }
        };
    });
}
else if (builder.Environment.IsProduction())
{
    throw new Exception("JWT secret key is required in production");
}

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("whitelistedURLs");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors("whitelistedURLs");
app.Run();