using MadameCoco.Order.API.Features.Order.Results;
using MadameCoco.Shared.BaseModels;
using MediatR;

namespace MadameCoco.Order.API.Features.Order.Queries.OrderQueries
{
   public record GetAllOrdersQuery() : IRequest<ServiceResult<DetailListResponse<OrderResponseDto>>>;
}
