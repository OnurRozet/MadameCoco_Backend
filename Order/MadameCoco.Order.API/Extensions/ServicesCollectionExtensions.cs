using FluentValidation;
using MadameCoco.Order.API.Clients;
using MadameCoco.Order.API.Data;
using MadameCoco.Order.API.Features.Order.Commands.CreateOrder;
using MadameCoco.Order.API.Interfaces;
using MadameCoco.Order.API.Mapping;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MadameCoco.Order.API.Extensions
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddOrderServiceDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. DB Context Kaydı (InMemory seçeneği testler için)
            var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
            if (useInMemory)
            {
                var dbName = configuration.GetValue<string>("InMemoryDbName") ?? "OrderIntegrationTestDb";
                services.AddDbContext<OrderDbContext>(options => options.UseInMemoryDatabase(dbName));
            }
            else
            {
                services.AddDbContext<OrderDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            }


            // 2. CQRS (MediatR) Kaydı
            var assemblyToScan = typeof(CreateOrderCommandHandler).Assembly;
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assemblyToScan));


            // 3. Validatör Kayıtları (FluentValidation)
            services.AddValidatorsFromAssembly(typeof(ServicesCollectionExtensions).Assembly);

            // 4. AutoMapper - Sadece belirli Profile tipini kullan (assembly taraması yapmaz)
            services.AddAutoMapper(typeof(MappingProfile));


            // 6. Customer Client Kaydı (HttpClientFactory)
            services.AddHttpClient<ICustomerClient, CustomerClient>(client =>
            {
                // Bu BaseAddress, genellikle API Gateway'in Customer API'ye yönlendirdiği adres olmalıdır.
                // Örnek: "http://customer.api.internal/" veya Development ortamında
                client.BaseAddress = new Uri(configuration["ServiceUrls:CustomerApi"] ?? "http://localhost:5000");
            });

            return services;
        }

        public static IServiceCollection AddControllersConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
            })
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
             });
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            return services;
        }

        public static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
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

            return services;
        }

        public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
              .AddDbContextCheck<MadameCoco.Order.API.Data.OrderDbContext>(
               name: "database",
               tags: new[] { "db", "sql", "ready" }
              );

            return services;
        }

        public static WebApplication AddHealthCheckEndpoints(this WebApplication app) {
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
