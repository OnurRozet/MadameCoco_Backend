using MadameCoco.Customer.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCustomerServiceDependencies(builder.Configuration);

// Health Checks - Servisin sağlık durumunu kontrol etmek için
// Bu endpoint Docker, Kubernetes ve load balancer'lar tarafından kullanılır
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MadameCoco.Customer.API.Data.CustomerDbContext>(
        name: "database", 
        tags: new[] { "db", "sql", "ready" });

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

app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
