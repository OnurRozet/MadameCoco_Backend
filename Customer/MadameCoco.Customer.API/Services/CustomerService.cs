using AutoMapper;
using FluentValidation;
using MadameCoco.Customer.API.DTOs;
using MadameCoco.Customer.API.Interfaces;
using MadameCoco.Shared.BaseEntities;
using MadameCoco.Shared.BaseModels;
using Microsoft.EntityFrameworkCore;

namespace MadameCoco.Customer.API.Services
{
    public class CustomerService(IUnitOfWork uow, IMapper mapper, IValidator<Entities.Customer> validator) : ICustomerService
    {
        public async Task<ServiceResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto)
        {
            try
            {
                await uow.BeginTransactionAsync();

                var customer = mapper.Map<Entities.Customer>(dto);
                if (customer is null) return ServiceResult<CustomerResponseDto>.Error("Müşteri Bulunamadı");

                var validationResult = validator.Validate(customer);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    return ServiceResult<CustomerResponseDto>.Error(errors);
                }
                await uow.CustomerRepository.CreateAsync(customer);

                await uow.CommitAndDisposeTransactionAsync();

                return ServiceResult<CustomerResponseDto>.Success(new CustomerResponseDto(
        customer.Id,
        customer.Name,
        customer.Email,
        mapper.Map<AddressDto>(customer.Address),
        customer.CreatedAt,
        customer.UpdatedAt
    ), message: "Başarılı şekilde oluşturulmuştur"
                );
            }
            catch (Exception)
            {
                await uow.RollbackTransactionAsync();
                throw;
            }

        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            var customer = await uow.CustomerRepository.FindAsync(id);
            if (customer is null) return ServiceResult<bool>.Error("Belirtilen id ye ait müşteri bulunamadı");
            uow.CustomerRepository.Delete(customer);
            return ServiceResult<bool>.Success(true, "Başarılı şekilde silindi.");
        }

        public async Task<ServiceResult<DetailListResponse<CustomerResponseDto>>> GetAllAsync()
        {
            var customer = await uow.CustomerRepository.GetAll().ToListAsync();
            var result = customer.Select(customer
                => new CustomerResponseDto(
        customer.Id,
        customer.Name,
        customer.Email,
        mapper.Map<AddressDto>(customer.Address),
        customer.CreatedAt,
        customer.UpdatedAt
    ));
            return ServiceResult<DetailListResponse<CustomerResponseDto>>.Success(new DetailListResponse<CustomerResponseDto>
            {
                SearchResult = [.. result],
                TotalItemCount = customer.Count,
            });
        }

        public async Task<ServiceResult<DetailResponse<CustomerResponseDto?>>> GetByIdAsync(Guid id)
        {
            var customer = await uow.CustomerRepository.FindAsync(id);
            if (customer is null)
            {
                return ServiceResult<DetailResponse<CustomerResponseDto?>>.Error("Belirtilen id ye ait müşteri bulunamadı");
            }
            var result = mapper.Map<CustomerResponseDto?>(customer);
            return ServiceResult<DetailResponse<CustomerResponseDto?>>.Success(new DetailResponse<CustomerResponseDto?>()
            {
                Detail = result
            });
        }

        public async Task<ServiceResult<bool>> UpdateAsync(UpdateCustomerDto dto)
        {
            var customer = mapper.Map<Entities.Customer>(dto);
            var existingCustomer = await uow.CustomerRepository.GetAll().Where(x => x.Id == customer.Id).FirstOrDefaultAsync();
            if (existingCustomer is null)
            {
                return ServiceResult<bool>.Error("Güncellenecek müşteri bulunamadı");
            }
            existingCustomer.Name = dto.Name;
            existingCustomer.Address = mapper.Map<Address>(dto.Address);
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            uow.CustomerRepository.Update(existingCustomer);

            return ServiceResult<bool>.Success(true, "Başarılı şekilde güncellendi");
        }

        public async Task<bool> ValidateAsync(Guid id)
        {
            return await uow.CustomerRepository.GetAll().AnyAsync(c => c.Id == id);
        }
    }
}
