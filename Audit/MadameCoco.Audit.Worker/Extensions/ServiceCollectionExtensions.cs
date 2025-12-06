using Hangfire;
using Hangfire.Redis.StackExchange;
using MadameCoco.Audit.Worker.Consumers;
using MadameCoco.Audit.Worker.Interfaces;
using MadameCoco.Audit.Worker.Models;
using MadameCoco.Audit.Worker.Services;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MadameCoco.Audit.Worker.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddAuditWorkerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. SERVİS KAYITLARI (Mail, Formatlayıcı, Raporlama)
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<ILogReportingService, LogReportingService>();

            // 2. MONGODB KAYDI
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = configuration["MongoDbSettings:ConnectionString"];
                return new MongoClient(settings);
            });

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var dbName = configuration["MongoDbSettings:DatabaseName"];
                return client.GetDatabase(dbName);
            });

            return services;
        }
        public static IServiceCollection AddAndConfigureHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Raporlama Servisini Interface ile Kaydetme  
            services.AddScoped<ILogReportingService, LogReportingService>();

            // 2.HANGFIRE KURULUMU (REDIS)  
            var redisConnection = configuration.GetConnectionString("RedisConnection");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseRecommendedSerializerSettings()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRedisStorage(redisConnection, new RedisStorageOptions
                {
                    // Görevlerin Redis'te saklanacağı anahtarın ön eki  
                    Prefix = "madamecoco:audit:hangfire:",
                    // Önemli: Redis'te görevlerin kalıcı olmasını sağlar (Redis ayarlarıyla da ilgili)  
                    InvisibilityTimeout = TimeSpan.FromHours(5)
                }));

            services.AddHangfireServer();

            return services;
        }
        public static IServiceCollection AddMassTransitWorker(this IServiceCollection services, IConfiguration configuration)
        {
            // MASSTRANSIT VE CONSUMER KAYDI
            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCreatedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                    var rabbitMqUser = configuration["RabbitMQ:User"] ?? "guest";
                    var rabbitMqPass = configuration["RabbitMQ:Pass"] ?? "guest";

                    cfg.Host(rabbitMqHost, "/", h =>
                    {
                        h.Username(rabbitMqUser);
                        h.Password(rabbitMqPass);
                    });

                    cfg.ReceiveEndpoint("order-created-audit-queue", e =>
                    {
                        e.ConfigureConsumer<OrderCreatedConsumer>(context);
                    });
                });
            });

            return services;
        }
        public static IServiceCollection AddMongoAndRabbitMqHealtCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var mongoConnectionString = configuration["MongoDbSettings:ConnectionString"];
            var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
            var rabbitMqUser = configuration["RabbitMQ:User"] ?? "guest";
            var rabbitMqPass = configuration["RabbitMQ:Pass"] ?? "guest";

            string[] MongoDbTags = new[] { "db", "mongodb", "ready" };
            string[] RabbitMqTags = new[] { "queue", "rabbitmq", "ready" };

            services.AddHealthChecks()
              .AddMongoDb(
                  sp => sp.GetRequiredService<IMongoClient>(),
                  name: "mongodb",
                  tags: MongoDbTags
              )
              .AddRabbitMQ(
                 sp => new RabbitMQ.Client.ConnectionFactory
                 {
                     Uri = new Uri($"amqp://{rabbitMqUser}:{rabbitMqPass}@{rabbitMqHost}/")
                 }.CreateConnectionAsync(), // Fix: Provide a factory function to resolve RabbitMQ connection  
                 name: "rabbitmq",
                 tags: RabbitMqTags
              );

            return services;
        }
        public static WebApplication AddHealthCheckEndpoints(this WebApplication app)
        {
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });
            app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => false
            });
            return app;
        }
    }
}
