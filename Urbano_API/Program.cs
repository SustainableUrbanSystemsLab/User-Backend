using Urbano_API.Models;
using Urbano_API.Services;
using Urbano_API.Repositories;
using Urbano_API.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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

