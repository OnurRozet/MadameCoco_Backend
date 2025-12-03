using MadameCoco.Customer.API.DTOs;
using MadameCoco.Customer.API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MadameCoco.Customer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(ICustomerService customerService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await customerService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var customer = await customerService.GetByIdAsync(id);
            return !customer.IsSuccess ? NotFound() : Ok(customer);
        }

        // Sipariş servisi müşterinin varlığını buradan kontrol edecek
        [HttpGet("validate/{id}")]
        public async Task<IActionResult> Validate(Guid id)
        {
            var exists = await customerService.ValidateAsync(id);
            return Ok(exists);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerDto dto)
        {
            var result = await customerService.CreateAsync(dto);
            return !result.IsSuccess ? BadRequest(result) : Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateCustomerDto dto)
        {
            var result = await customerService.UpdateAsync(dto);
            return !result.IsSuccess ? BadRequest(result) : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await customerService.DeleteAsync(id);
            return !result.IsSuccess ? BadRequest(result) : Ok(result);
        }
    }
}
