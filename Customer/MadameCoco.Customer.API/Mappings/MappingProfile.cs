using AutoMapper;
using MadameCoco.Customer.API.DTOs;
using MadameCoco.Customer.API.Entities;
using MadameCoco.Shared.BaseEntities;

namespace MadameCoco.Customer.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Entities.Customer, CreateCustomerDto>().ReverseMap();
            CreateMap<Entities.Customer, UpdateCustomerDto>().ReverseMap();
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<Entities.Customer, CustomerResponseDto>();
        }
    }
}
