using AutoMapper;
using MadameCoco.Order.API.Data;
using MadameCoco.Order.API.Features.Order.Results;
using MadameCoco.Shared.BaseModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MadameCoco.Order.API.Features.Order.Queries.OrderQueries
{
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, ServiceResult<DetailListResponse<OrderResponseDto>>>
    {

        private readonly OrderDbContext _context;
        private readonly IMapper _mapper;

        public GetAllOrdersQueryHandler(OrderDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResult<DetailListResponse<OrderResponseDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            var resultDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);

            return ServiceResult<DetailListResponse<OrderResponseDto>>.Success(new DetailListResponse<OrderResponseDto>
            {
                SearchResult = resultDtos.ToList(),
                TotalItemCount = orders.Count
            });
        }
    }
}
