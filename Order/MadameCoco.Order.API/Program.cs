using MadameCoco.Order.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersConfiguration(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOrderServiceDependencies(builder.Configuration);
builder.Services.AddMassTransitConfiguration(builder.Configuration);

// Health Checks - Servisin sağlık durumunu kontrol etmek için


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
