using FluentValidation;
using MadameCoco.Order.API.Clients;
using MadameCoco.Order.API.Data;
using MadameCoco.Order.API.Features.Order.Commands.CreateOrder;
using MadameCoco.Order.API.Interfaces;
using MadameCoco.Order.API.Mapping;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MadameCoco.Order.API.Extensions
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddOrderServiceDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. DB Context Kaydı
            services.AddDbContext<OrderDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


            // 2. CQRS (MediatR) Kaydı
            var assemblyToScan = typeof(CreateOrderCommandHandler).Assembly;
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assemblyToScan));


            // 3. Validatör Kayıtları (FluentValidation)
            services.AddValidatorsFromAssembly(typeof(ServicesCollectionExtensions).Assembly);

            // 4. AutoMapper - Sadece belirli Profile tipini kullan (assembly taraması yapmaz)
            services.AddAutoMapper(typeof(MappingProfile));

            // 5. RabbitMQ (MassTransit) Kaydı
            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(ServicesCollectionExtensions).Assembly);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                    var rabbitMqUser = configuration["RabbitMQ:User"] ?? "guest";
                    var rabbitMqPass = configuration["RabbitMQ:Pass"] ?? "guest";

                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username(rabbitMqUser);
                        h.Password(rabbitMqPass);
                    });
                    cfg.ConfigureEndpoints(context);

                });
            });

            // 6. Customer Client Kaydı (HttpClientFactory)
            services.AddHttpClient<ICustomerClient, CustomerClient>(client =>
            {
                // Bu BaseAddress, genellikle API Gateway'in Customer API'ye yönlendirdiği adres olmalıdır.
                // Örnek: "http://customer.api.internal/" veya Development ortamında
                client.BaseAddress = new Uri(configuration["ServiceUrls:CustomerApi"] ?? "http://localhost:5000");
            });

            return services;
        }
    }
}
