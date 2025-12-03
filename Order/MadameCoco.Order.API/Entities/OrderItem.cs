namespace MadameCoco.Order.API.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
