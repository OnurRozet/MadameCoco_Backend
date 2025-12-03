using MadameCoco.Order.API.Entities.Enums;
using MadameCoco.Shared.BaseEntities;

namespace MadameCoco.Order.API.Features.Order.Results
{
   // Sipariş Kalemi (Item) Response DTO
    public record OrderItemResponseDto(
        Guid ProductId,
        string ProductName,
        string ImageUrl,
        int Quantity,
        double Price
    );

    // Ana Sipariş Response DTO  
    public record OrderResponseDto(
        Guid Id,
        Guid CustomerId,
        OrderStatus Status,
        Shared.BaseEntities.Address ShippingAddress,
        List<OrderItemResponseDto> Items,
        double TotalPrice,
        DateTime CreatedAt
    );
}
