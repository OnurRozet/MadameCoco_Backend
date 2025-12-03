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
            // 1. DB Context Kaydı
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<CustomerDbContext>(options =>
                options.UseSqlServer(connectionString));

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
    }
}
