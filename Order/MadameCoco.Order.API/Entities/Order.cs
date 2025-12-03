using MadameCoco.Order.API.Entities.Enums;
using MadameCoco.Shared.BaseEntities;

namespace MadameCoco.Order.API.Entities
{
    public class Order : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Address ShippingAddress { get; set; } = default!;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalPrice => Items.Sum(i => i.Quantity * i.Price);
    }
}
