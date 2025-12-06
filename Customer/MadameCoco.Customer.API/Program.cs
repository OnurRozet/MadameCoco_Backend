using MadameCoco.Customer.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersConfiguration(builder.Configuration);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCustomerServiceDependencies(builder.Configuration);

// Health Checks - Servisin sağlık durumunu kontrol etmek için
// Bu endpoint Docker, Kubernetes ve load balancer'lar tarafından kullanılır
builder.Services.AddCustomerServiceHealthCheckConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Health Check Endpoint'leri
app.AddHealthCheckEndpoints();

app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
//public partial class Program { }
