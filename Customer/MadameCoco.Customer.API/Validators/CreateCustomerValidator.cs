using FluentValidation;

namespace MadameCoco.Customer.API.Validators
{
    public class CreateCustomerValidator : AbstractValidator<Entities.Customer>
    {
        public CreateCustomerValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("İsim boş olamaz.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir email giriniz.");

            // Address validasyonları
            RuleFor(x => x.Address).NotNull().WithMessage("Adres gereklidir.");
            RuleFor(x => x.Address.City).NotEmpty().WithMessage("Şehir gereklidir.");
            RuleFor(x => x.Address.Country).NotEmpty().WithMessage("Ülke gereklidir.");
            RuleFor(x => x.Address.CityCode).GreaterThan(0).WithMessage("Poosta kodu gereklidir.");
        }
    }
}
