using MadameCoco.Customer.API.Data;
using MadameCoco.Customer.API.Interfaces;
using MadameCoco.Shared.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MadameCoco.Customer.API.Services
{
    /// <summary>
    /// Customer mikroservisine özel generic repository implementasyonu.
    /// </summary>
    public class Repository<T>(CustomerDbContext context) : IRepository<T> where T : BaseEntity,new()
    {
        public async Task CreateAsync(T entity)
        {
            await context.Set<T>().AddAsync(entity);
            entity.CreatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        public void Delete(T entity)
        {
            context.Set<T>().Remove(entity);
            context.SaveChanges();
        }

        public async Task<T?> FindAsync(Guid id)
        {
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return await context.Set<T>().FindAsync(id);
        }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[] properties)
        {
            var query = context.Set<T>().AsNoTracking();
            if (properties.Length != 0)
                query = properties.Aggregate(query, (current, property) => current.Include(property));
            return query;
        }

        public void Update(T entity)
        {
            context.Set<T>().Update(entity);
            entity.UpdatedAt = DateTime.UtcNow;
            context.SaveChanges();
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> where)
        {
            return context.Set<T>().Where(where);
        }
    }
}
