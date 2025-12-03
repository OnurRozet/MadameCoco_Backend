using MadameCoco.Order.API.DTOs;
using MadameCoco.Shared.BaseEntities;
using MadameCoco.Shared.BaseModels;
using MediatR;

namespace MadameCoco.Order.API.Features.Order.Commands.OrderCommands
{
    public record CreateOrderCommand(
       Guid CustomerId,
       Address ShippingAddress,
       List<OrderItemDto> Items
   ) : IRequest<ServiceResult<Guid>>;
}
