using MadameCoco.Order.API.Interfaces;

namespace MadameCoco.Order.API.Clients;

public class CustomerClient : ICustomerClient
{
    private readonly HttpClient _httpClient;
    public CustomerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateCustomerExistsAsync(Guid customerId)
    {
        var response = await _httpClient.GetAsync($"/customers/validate/{customerId}");

        if (response.IsSuccessStatusCode)
        {
            // Müşteri bulundu (2xx kodu)
            return true;
        }

        // 404, 500 vb. durumlarda false dön
        return false;
    }
}