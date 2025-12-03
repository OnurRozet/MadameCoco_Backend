namespace MadameCoco.Order.API.Interfaces
{
    public interface ICustomerClient
    {
        Task<bool> ValidateCustomerExistsAsync(Guid customerId);
    }
}
