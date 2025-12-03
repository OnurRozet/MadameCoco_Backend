namespace MadameCoco.Order.API.DTOs
{
    public record OrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
