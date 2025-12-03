namespace MadameCoco.Customer.API.DTOs
{
    public record CreateCustomerDto(
     string Name,
     string Email,
     AddressDto Address
    );

    public record UpdateCustomerDto(
        Guid Id,
        string Name,
        AddressDto Address
    );

    public record AddressDto(
        string AddressLine,
        string City,
        string Country,
        int CityCode
    );

    public record CustomerResponseDto(
        Guid Id,
        string Name,
        string Email,
        AddressDto Address,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
