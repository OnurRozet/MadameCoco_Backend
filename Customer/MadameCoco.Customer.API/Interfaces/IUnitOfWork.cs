using Microsoft.EntityFrameworkCore.Storage;

namespace MadameCoco.Customer.API.Interfaces
{
    /// <summary>
    /// Customer mikroservisi için UnitOfWork sözleşmesi.
    /// </summary>
    public interface IUnitOfWork
    {
        IRepository<Entities.Customer> CustomerRepository { get; }
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAndDisposeTransactionAsync();
        Task RollbackTransactionAsync();
    }
}


