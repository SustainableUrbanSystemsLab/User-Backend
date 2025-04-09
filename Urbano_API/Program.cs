using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Urbano_API.Models;
using Urbano_API.Services;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;
using System.Security.Claims;
using Microsoft.IdentityModel.Logging;
using System.Text.Json.Serialization; // Add this at the top

IdentityModelEventSource.ShowPII = true; // Enable detailed JWT errors

var builder = WebApplication.CreateBuilder(args);

// Load .env
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

// Get EV vars
var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
var smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT");
var mailingEmailFrom = Environment.GetEnvironmentVariable("MAILING_EMAILFROM");
var mailingEmailName = Environment.GetEnvironmentVariable("MAILING_EMAILNAME");
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTIONSTRING");
var jwtKey = Environment.GetEnvironmentVariable("SECRETKEY");
var salt = Environment.GetEnvironmentVariable("PASSWORDSALT");

Console.WriteLine($"Key: '{jwtKey}'");
Console.WriteLine($"Length: {jwtKey.Length} chars");
Console.WriteLine($"Byte Size: {Encoding.UTF8.GetBytes(jwtKey).Length} bytes");
Console.WriteLine($"Bit Size: {Encoding.UTF8.GetBytes(jwtKey).Length * 8} bits");

// Replace with EV vars
builder.Configuration["Mailing:SmtpUsername"] = smtpUsername;
builder.Configuration["Mailing:SmtpPassword"] = smtpPassword;
builder.Configuration["Mailing:SmtpServer"] = smtpServer;
builder.Configuration["Mailing:SmtpPort"] = smtpPort;
builder.Configuration["Mailing:FromEmail"] = mailingEmailFrom;
builder.Configuration["Mailing:FromName"] = mailingEmailName;
builder.Configuration["UrbanoDatabase:ConnectionString"] = mongoConnectionString;
builder.Configuration["SecretKey"] = jwtKey;
builder.Configuration["passwordSalt"] = salt;

builder.Configuration["Jwt:Key"] = jwtKey; // Add JWT key to configuration // Not sure why, Keeping For Now, Secreting Key hurt me T_T 

// EV Loading Warning
if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort))
{
    Console.WriteLine("Warning: SMTP username, password, server, and/or port is missing. Emails may not be sent.");
}

if (string.IsNullOrEmpty(mailingEmailFrom) || string.IsNullOrEmpty(mailingEmailName))
{
    Console.WriteLine("Warning: email and/or email name is missing. Emails may not be sent and/or propper.");
}

if (string.IsNullOrEmpty(mongoConnectionString))
{
    Console.WriteLine("Warning: MongoDB Connection String is missing.");
}

if (string.IsNullOrEmpty(jwtKey))
{
    Console.WriteLine("Warning: JWT key is missing. Authentication will not work.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "whitelistedURLs",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173",
                                              "https://localhost:5173",
                                              "https://urbano-frontend.onrender.com",
                                              "https://hoppscotch.io")
                                                   .AllowAnyHeader()
                                                  .AllowAnyMethod();
                      });
});

// Add services to the container.
builder.Services.Configure<UrbanoStoreDatabaseSettings>(
    builder.Configuration.GetSection("UrbanoDatabase"));

builder.Services.Configure<UrbanoStoreEmailSettings>(
    builder.Configuration.GetSection("Mailing"));

builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IVerificationService, VerificationService>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IVerificationRepository, VerificationRepository>();
builder.Services.AddSingleton<IMetricsRepository, MetricsRepository>();
builder.Services.AddSingleton<IRegistrationsRepository, RegistrationsRepository>();
builder.Services.AddSingleton<ISimulationsRepository, SimulationsRepository>();
builder.Services.AddSingleton<IWalletRepository, WalletRepository>();
builder.Services.AddSingleton<ILoginsRepository, LoginsRepository>();

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
            context.HandleResponse(); // Prevents the default redirect response
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

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("whitelistedURLs");

// Ensure Authentication is called before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCors("whitelistedURLs");

app.Run();