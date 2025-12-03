using AutoMapper;
using MediatR;
using MadameCoco.Order.API.Data;
using MadameCoco.Order.API.Entities;
using MadameCoco.Shared.BaseModels;
using Microsoft.EntityFrameworkCore;
using MadameCoco.Order.API.Features.Order.Queries.OrderQueries;
using MadameCoco.Order.API.Features.Order.Results;

namespace MadameCoco.Order.API.Features.Queries;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ServiceResult<DetailResponse<OrderResponseDto?>>>
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(OrderDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceResult<DetailResponse<OrderResponseDto?>>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return ServiceResult<DetailResponse<OrderResponseDto?>>.Error("Belirtilen ID'ye ait sipariş bulunamadı.");
        }

        var resultDto = _mapper.Map<OrderResponseDto>(order);

        return ServiceResult<DetailResponse<OrderResponseDto?>>.Success(new DetailResponse<OrderResponseDto?>()
        {
            Detail = resultDto
        });
    }
}