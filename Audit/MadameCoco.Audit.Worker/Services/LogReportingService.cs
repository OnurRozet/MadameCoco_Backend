using MadameCoco.Audit.Worker.Entities;
using MadameCoco.Audit.Worker.Interfaces;
using MadameCoco.Shared.IntegrationEvents;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadameCoco.Audit.Worker.Services
{
    public class LogReportingService : ILogReportingService
    {
        private readonly IMongoCollection<OrderLog> _orderLogs;
        private readonly ILogger<LogReportingService> _logger;
        private readonly IEmailService _emailService;

        public LogReportingService(IMongoDatabase database, IConfiguration configuration, ILogger<LogReportingService> logger, IEmailService emailService)
        {
            _logger = logger;
            // Audit.Worker/appsettings.json'dan koleksiyon adını alır  
            var collectionName = configuration["MongoDbSettings:CollectionName"];
            _orderLogs = database.GetCollection<OrderLog>(collectionName);
            _emailService = emailService;
        }

        public async Task SendDailyReportAsync()
        {
            _logger.LogInformation($"[HANGFIRE] Raporlama Görevi Başladı. Zaman: {DateTime.UtcNow}");

            //1. MongoDb den veri çekelim
            var timeThreshold = DateTime.Now.AddMinutes(-10);

            var recentLogs = await _orderLogs
            .Find(log => log.CreatedAt >= timeThreshold)
            .ToListAsync();

            _logger.LogWarning($"[MONGODB] Son 10 dakikada {recentLogs.Count} adet log olayı bulundu.");

            if (recentLogs.Count > 0)
            {
                var (subject, body) = _emailService.FormatReport(recentLogs, DateTime.Now);

                await _emailService.SendEmailAsync(subject, body);

                _logger.LogCritical($"[RAPOR] {recentLogs.Count} adet olay loglandı. Mail gönderim işlemi tamamlandı.");
            }
            else
            {
                _logger.LogInformation("[RAPOR] Son 10 dakikada yeni bir işlem yok.");
            }

            _logger.LogInformation("[HANGFIRE] Raporlama Görevi Başarıyla Tamamlandı.");
        }
    }
}
