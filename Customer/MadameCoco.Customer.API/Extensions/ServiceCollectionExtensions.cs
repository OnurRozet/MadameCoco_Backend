using AutoMapper;
using FluentValidation;
using MadameCoco.Customer.API.Data;
using MadameCoco.Customer.API.DTOs;
using MadameCoco.Customer.API.Interfaces;
using MadameCoco.Customer.API.Mapping;
using MadameCoco.Customer.API.Services;
using MadameCoco.Customer.API.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MadameCoco.Customer.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomerServiceDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. DB Context Kaydı (testler için InMemory destekli)
            var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
            if (useInMemory)
            {
                var dbName = configuration.GetValue<string>("InMemoryDbName") ?? "IntegrationTestDb";
                services.AddDbContext<CustomerDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<CustomerDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // 2. İş Servisleri Kaydı
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // FluentValidation - Assembly taraması yerine sadece belirli validator tipini kullan
            services.AddValidatorsFromAssembly(typeof(CreateCustomerValidator).Assembly);

            // AutoMapper - Sadece belirli Profile tipini kullan (assembly taraması yapmaz)
            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }

        public static IServiceCollection AddCustomerServiceHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
               .AddDbContextCheck<MadameCoco.Customer.API.Data.CustomerDbContext>(
                name: "database",
                tags: new[] { "db", "sql", "ready" }
               );

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

        public static WebApplication AddHealthCheckEndpoints(this WebApplication app)
        {
            // /health - Genel sağlık durumu (hızlı kontrol)
            // /health/ready - Servis hazır mı? (database bağlantısı dahil)
            // /health/live - Servis çalışıyor mu? (basit kontrol)
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });
            app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => false // Hiçbir check çalıştırma, sadece servisin ayakta olduğunu kontrol et
            });

            return app;
        }
    }

}
