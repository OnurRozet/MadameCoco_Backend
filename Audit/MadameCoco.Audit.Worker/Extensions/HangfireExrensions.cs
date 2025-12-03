using Hangfire;
// 💡 Redis için gerekli using
using Hangfire.Redis.StackExchange;
using MadameCoco.Audit.Worker.Interfaces;
using MadameCoco.Audit.Worker.Services;
// ... diğer using'ler ...  

public static class HangfireExtensions
{
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

    public static IHost ConfigureHangfireRecurringJobs(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            // 💡 Interface üzerinden servisi çağırıyoruz.  
            var reportingService = scope.ServiceProvider.GetRequiredService<ILogReportingService>();

            // 💡 DELAYED (TEKRARLAYAN) GÖREV TANIMLAMA 💡  
            // Şimdilik 1 dakikada bir çalışacak Cron ifadesini kullanıyoruz.  
            RecurringJob.AddOrUpdate<ILogReportingService>(
                "TestDailyLogReport",
                service => service.SendDailyReportAsync(),
                "*/1 * * * *", // Cron: Her 1 dakikada bir (Test amaçlı)  
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Utc
                }
            );

            // NOT: Günlük 03:00 için Cron ifadesi: "0 3 * * *" olmalıdır.  
        }
        return host;
    }
}
