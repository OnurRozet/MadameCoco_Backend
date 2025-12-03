using System.Linq.Expressions;
using MadameCoco.Shared.BaseEntities;

namespace MadameCoco.Customer.API.Interfaces
{
    /// <summary>
    /// Customer mikroservisinde kullanılacak generic repository sözleşmesi.
    /// </summary>
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> FindAsync(Guid id);
        IQueryable<T> GetAll(params Expression<Func<T, object>>[] properties);
        IQueryable<T> Where(Expression<Func<T, bool>> where);
        Task CreateAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}


