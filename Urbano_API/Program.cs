using Urbano_API.Models;
using Urbano_API.Services;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Load .env
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

// Get EV vars
var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

// Replace with EV vars
builder.Configuration["Mailing:SmtpUsername"] = smtpUsername;
builder.Configuration["Mailing:SmtpPassword"] = smtpPassword;

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

app.UseAuthorization();

app.MapControllers();

app.UseCors("whitelistedURLs");

app.Run();

