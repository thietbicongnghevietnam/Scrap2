using Serilog;
using Microsoft.OpenApi.Models;
using ScrapSystem.Api.Infrastructure.Configuration;
using ScrapSystem;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Repositories;


var builder = WebApplication.CreateBuilder(args);

//SeriLog configuration
Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Debug()
          .WriteTo.Console()
          .WriteTo.File($"{AppDomain.CurrentDomain.BaseDirectory}/logs/log-.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
          .CreateLogger();

//doan ket noi co so du lieu  31.07.2025
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Server=10.92.186.30;Database=ScrapSystem;User Id=sa;Password=Psnvdb2013;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;")));


builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", builder =>
    {
        builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowMVC");

app.UseAuthentication();

app.UseAuthorization();
app.UseExceptionHandler("/error");
app.UseJwtBlacklist();


app.MapControllers();

app.Run();
