using MadameCoco.Order.API.Entities.Enums;
using MadameCoco.Order.API.Entities;
using MadameCoco.Shared.BaseModels;
using MassTransit;
using MassTransit.Transports;
using MediatR;
using MadameCoco.Shared.BaseEntities;
using MadameCoco.Order.API.Data;
using MadameCoco.Order.API.Interfaces;
using MadameCoco.Order.API.Features.Order.Commands.OrderCommands;
using MadameCoco.Shared.IntegrationEvents;

namespace MadameCoco.Order.API.Features.Order.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ServiceResult<Guid>>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly OrderDbContext _orderDb;
        private readonly ICustomerClient _customerClient;

        public CreateOrderCommandHandler(IPublishEndpoint publishEndpoint, OrderDbContext orderDb, ICustomerClient customerClient)
        {
            _publishEndpoint = publishEndpoint;
            _orderDb = orderDb;
            _customerClient = customerClient;
        }

        public async Task<ServiceResult<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // ADIM 1: İş Doğrulamaları  
            var customerExists = await _customerClient.ValidateCustomerExistsAsync(request.CustomerId);
            if (!customerExists) return ServiceResult<Guid>.Error("Müşteri bulunamadı.");

            // ADIM 2: Order Entity'sini Oluşturma  
            var order = new Entities.Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now,
                ShippingAddress = new Address
                {
                    City = request.ShippingAddress.City,
                    Country = request.ShippingAddress.Country,
                    CityCode = request.ShippingAddress.CityCode,
                    AddressLine = request.ShippingAddress.AddressLine
                },
                Items = request.Items.Select(itemDto => new OrderItem
                {
                    OrderId = Guid.NewGuid(),
                    ProductId = itemDto.ProductId,
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    Price = itemDto.Price,
                    ImageUrl = itemDto.ImageUrl,
                }).ToList()
            };

            await _orderDb.AddAsync(order);
            await _orderDb.SaveChangesAsync();

            // ADIM 4: ASENKRON İLETİŞİM (Event Fırlatma)  
            // Audit Worker'ın dinlemesi için RabbitMQ'ya Event fırlatıyoruz.  

            await _publishEndpoint.Publish(new OrderCreatedEvent( // Event modelini Shared'da tanımlayacağız  
              order.Id,
              order.CustomerId,
              order.Items.Select(x => x.ProductId).First(),
              string.Join(", ", order.Items.Select(x => x.ProductName)), // Fix: Convert List<string> to a single string  
              order.Items.Sum(x => x.Quantity),
              order.TotalPrice,
              order.CreatedAt
            ), cancellationToken);

            return ServiceResult<Guid>.Success(order.Id);
        }
    }
}
