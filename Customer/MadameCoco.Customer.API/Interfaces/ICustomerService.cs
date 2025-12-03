using MadameCoco.Customer.API.DTOs;
using MadameCoco.Shared.BaseModels;

namespace MadameCoco.Customer.API.Interfaces
{
    public interface ICustomerService
    {
        Task<ServiceResult<DetailListResponse<CustomerResponseDto>>> GetAllAsync();
        Task<ServiceResult<DetailResponse<CustomerResponseDto?>>> GetByIdAsync(Guid id);
        Task<ServiceResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto);
        Task<ServiceResult<bool>> UpdateAsync(UpdateCustomerDto dto);
        Task<ServiceResult<bool>> DeleteAsync(Guid id);
        Task<bool> ValidateAsync(Guid id);
    }
}
