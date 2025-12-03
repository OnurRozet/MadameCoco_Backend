using AutoMapper;
using MadameCoco.Order.API.Features.Order.Results;

namespace MadameCoco.Order.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Entities.Order, OrderResponseDto>().ReverseMap();
            CreateMap<Entities.Order, OrderItemResponseDto>().ReverseMap();
        }
    }
}
