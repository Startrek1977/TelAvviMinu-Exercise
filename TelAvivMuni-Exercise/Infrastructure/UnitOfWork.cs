using System;
using System.Threading.Tasks;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// Coordinates operations across repositories and manages persistence.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductRepository _productRepository;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of UnitOfWork with a default ProductRepository.
        /// </summary>
        public UnitOfWork()
            : this(new ProductRepository())
        {
        }

        /// <summary>
        /// Initializes a new instance of UnitOfWork with a custom repository.
        /// </summary>
        /// <param name="productRepository">The product repository to use.</param>
        public UnitOfWork(ProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        /// <inheritdoc />
        public IRepository<Product> Products => _productRepository;

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync()
        {
            return await _productRepository.SaveAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources if any
                }
                _disposed = true;
            }
        }
    }
}
