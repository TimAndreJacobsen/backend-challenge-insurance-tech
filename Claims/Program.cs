using Claims.Auditing;
using Claims.Infrastructure;
using Claims.Persistence;
using Claims.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;

var builder = WebApplication.CreateBuilder(args);

// Start Testcontainers for SQL Server and MongoDB
var sqlContainer = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        : new()

    ).Build();

var mongoContainer = new MongoDbBuilder()
    .WithImage("mongo:latest")
    .Build();

await sqlContainer.StartAsync();
await mongoContainer.StartAsync();

// Add services to the container.
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<Claims.Infrastructure.FluentValidationFilter>();
    })
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<AuditContext>(options =>
    options.UseSqlServer(sqlContainer.GetConnectionString()));

var mongoClient = new MongoClient(mongoContainer.GetConnectionString());
var mongoDatabase = mongoClient.GetDatabase(builder.Configuration["MongoDb:DatabaseName"]);

builder.Services.AddDbContext<ClaimsContext>(options =>
    options.UseMongoDB(mongoDatabase.Client, mongoDatabase.DatabaseNamespace.DatabaseName));

// In-memory channel for audit messages. Needs refactoring before production for scalability and to prevent data loss.
// See TODO's in AuditBackgroundService.cs
var auditChannel = Channel.CreateUnbounded<AuditMessage>();
builder.Services.AddSingleton(auditChannel.Reader);
builder.Services.AddSingleton(auditChannel.Writer);
builder.Services.AddHostedService<AuditBackgroundService>();

builder.Services.AddScoped<IAuditer, Auditer>();
builder.Services.AddScoped<IClaimsService, ClaimsService>();
builder.Services.AddScoped<ICoversService, CoversService>();
builder.Services.AddSingleton<IPremiumCalculator, PremiumCalculator>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
    await context.Database.MigrateAsync();
}

await app.RunAsync();

public partial class Program { }
