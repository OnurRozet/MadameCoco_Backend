using MassTransit;
using MadameCoco.Audit.Worker.Consumers;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MadameCoco.Audit.Worker.Models;
using MadameCoco.Audit.Worker.Interfaces;
using MadameCoco.Audit.Worker.Services;
using Hangfire;
using MadameCoco.Audit.Worker.Extensions;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// 1. Temel Audit Servislerini (Mail, MongoDB vb.) kaydet
services.AddAuditWorkerServices(configuration);

// 2. Hangfire'ı (Redis ile) kaydet
services.AddAndConfigureHangfire(configuration);

// 3. MassTransit (RabbitMQ) ile Consumer'ları kaydet
services.AddMassTransitWorker(configuration);

// 4. Health Checks - MongoDB ve RabbitMQ bağlantılarını kontrol et
var mongoConnectionString = configuration["MongoDbSettings:ConnectionString"];
var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitMqUser = configuration["RabbitMQ:User"] ?? "guest";
var rabbitMqPass = configuration["RabbitMQ:Pass"] ?? "guest";

services.AddHealthChecks()
    .AddMongoDb(mongoConnectionString ?? string.Empty, name: "mongodb", tags: new[] { "db", "mongodb", "ready" })
    .AddRabbitMQ(
        rabbitConnectionString: $"amqp://{rabbitMqUser}:{rabbitMqPass}@{rabbitMqHost}/",
        name: "rabbitmq",
        tags: new[] { "queue", "rabbitmq", "ready" });

//5. Web Uygulamasını İnşa Et 💡
var app = builder.Build();


var options = new DashboardOptions { };
app.UseHangfireDashboard("/hangfire", options);

// Health Check Endpoint'leri
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// 💡 ADIM 6: Periyodik Görevleri Başlat 💡
app.ConfigureRecurringJobs();

app.Run();