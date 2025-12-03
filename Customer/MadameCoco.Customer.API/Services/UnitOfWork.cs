using MadameCoco.Customer.API.Data;
using MadameCoco.Customer.API.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace MadameCoco.Customer.API.Services
{
    /// <summary>
    /// Customer mikroservisine özel UnitOfWork implementasyonu.
    /// Sadece CustomerDbContext ve Customer repository'sini yönetir.
    /// </summary>
    public class UnitOfWork(CustomerDbContext customerDb) : IUnitOfWork
    {
        private IRepository<Entities.Customer>? _customerRepository;

        public IRepository<Entities.Customer> CustomerRepository =>
            _customerRepository ??= new Repository<Entities.Customer>(customerDb);

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            try
            {
                if (customerDb.Database.CurrentTransaction is not null)
                {
                    return customerDb.Database.CurrentTransaction;
                }
                return await customerDb.Database.BeginTransactionAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Transaction başlatılırken hata oluştu", ex);
            }
        }

        public async Task CommitAndDisposeTransactionAsync()
        {
            try
            {
                if (customerDb.Database.CurrentTransaction is not null)
                    await customerDb.Database.CurrentTransaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (customerDb.Database.CurrentTransaction is not null)
                {
                    await customerDb.Database.RollbackTransactionAsync();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
