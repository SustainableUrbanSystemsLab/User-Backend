using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Urbano_API.Models;
using Urbano_API.Services;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;
using System.Security.Claims;
using Microsoft.IdentityModel.Logging; // Add this at the top

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
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTIONSTRING");
var jwtKey = builder.Configuration.GetValue<string>("SecretKey") ?? "YourSuperSecretKey";

// Replace with EV vars
builder.Configuration["Mailing:SmtpUsername"] = smtpUsername;
builder.Configuration["Mailing:SmtpPassword"] = smtpPassword;
builder.Configuration["UrbanoDatabase:ConnectionString"] = mongoConnectionString;
builder.Configuration["Jwt:Key"] = jwtKey; // Add JWT key to configuration

// EV Loading Warning
if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
{
    Console.WriteLine("Warning: SMTP username or password is missing. Emails may not be sent.");
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

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
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

builder.Services.AddControllers();
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

