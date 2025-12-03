using MadameCoco.Audit.Worker.Entities;
using MadameCoco.Shared.IntegrationEvents;
using MassTransit;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadameCoco.Audit.Worker.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IMongoCollection<OrderLog> _orderLogs;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(IMongoDatabase database, IConfiguration configuration, ILogger<OrderCreatedConsumer> logger)
        {
            _logger = logger;
            var collectionName = configuration["MongoDbSettings:CollectionName"];
            _orderLogs = database.GetCollection<OrderLog>(collectionName);
        }
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var incomingEvent = context.Message;
            _logger.LogInformation($"[AUDIT] Yeni Order Created Event alındı. OrderId: {incomingEvent.OrderId}");

            // Event verisini Log Entity'sine dönüştür
            var logEntry = new OrderLog
            {
                OrderId = incomingEvent.OrderId.ToString(),
                CustomerId = incomingEvent.CustomerId.ToString(),
                ProductId = incomingEvent.ProductId.ToString(),
                ProductName = incomingEvent.ProdcutName,
                Quantity = incomingEvent.Quantity,
                TotalPrice = incomingEvent.TotalPrice,
                CreatedAt = incomingEvent.CreatedAt,
                EventType = nameof(OrderCreatedEvent) 
            };

            // MongoDB'ye kaydet
            await _orderLogs.InsertOneAsync(logEntry);

            _logger.LogInformation($"[AUDIT] OrderId {incomingEvent.OrderId} MongoDB'ye başarıyla loglandı.");
        }
    }
}
