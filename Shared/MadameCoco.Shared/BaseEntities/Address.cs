namespace MadameCoco.Shared.BaseEntities
{
    public class Address 
    {
        public string? AddressLine { get; set; }
        public string City { get; set; } = default!;
        public string Country { get; set; } = default!;
        public int CityCode { get; set; }
    }
}
