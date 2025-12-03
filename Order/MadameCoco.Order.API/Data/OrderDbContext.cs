using MadameCoco.Order.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace MadameCoco.Order.API.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.Order> Orders { get; set; }
        public DbSet<Entities.OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entities.Order>().OwnsOne(x => x.ShippingAddress);
            modelBuilder.Entity<Entities.Order>().Property(p => p.Status).HasConversion<string>();
        }
    }
}
