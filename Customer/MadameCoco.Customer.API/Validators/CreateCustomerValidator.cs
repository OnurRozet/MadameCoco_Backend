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
            RuleFor(x => x.Address).NotNull();
            RuleFor(x => x.Address.City).NotEmpty();
            RuleFor(x => x.Address.Country).NotEmpty();
            RuleFor(x => x.Address.CityCode).GreaterThan(0);
        }
    }
}
