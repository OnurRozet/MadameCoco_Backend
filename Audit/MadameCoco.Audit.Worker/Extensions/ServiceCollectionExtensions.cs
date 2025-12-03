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

            // 💡 2. YENİ HANGFIRE KURULUMU (REDIS) 💡  
            var redisConnection = configuration.GetConnectionString("RedisConnection");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseRecommendedSerializerSettings()
                .UseSimpleAssemblyNameTypeSerializer()
                // 💡 Redis Depolamasını Kullan  
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

        public static WebApplication ConfigureRecurringJobs(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                // RecurringJob.AddOrUpdate for IRecurringJobManager using the updated method signature  
                var manager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

                // Updated to use RecurringJobOptions  
                var options = new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Utc
                };

                manager.AddOrUpdate<ILogReportingService>(
                    "DailyLogReport_10AM",
                    service => service.SendDailyReportAsync(),
                    "0/1 * * * *",
                    options
                );
            }
            return app;
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
    }
}
