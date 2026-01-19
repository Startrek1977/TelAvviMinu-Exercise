using System;
using System.Threading.Tasks;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Product> Products { get; }
        Task<int> SaveChangesAsync();
    }
}
