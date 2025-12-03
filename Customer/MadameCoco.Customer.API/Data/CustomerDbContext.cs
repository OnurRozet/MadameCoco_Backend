using MadameCoco.Customer.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace MadameCoco.Customer.API.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
        }
        public DbSet<Entities.Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Customer>().OwnsOne(x => x.Address);

            base.OnModelCreating(modelBuilder);
        }
    }
}
