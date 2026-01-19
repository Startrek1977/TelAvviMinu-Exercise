using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// Repository for Product entities.
    /// Named after the entity type it manages, with persistence strategy injected.
    /// Can use file-based, database, or any other IDataStore implementation.
    /// </summary>
    public class ProductRepository : IRepository<Product>
    {
        private static readonly string DefaultDataPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Products.json");

        private readonly IDataStore<Product> _dataStore;
        private List<Product> _entities = new();
        private bool _isLoaded;

        /// <summary>
        /// Initializes a new instance of the ProductRepository with the default file-based JSON storage.
        /// </summary>
        public ProductRepository()
            : this(new FileDataStore<Product>(DefaultDataPath, new JsonSerializer<Product>()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the ProductRepository with a custom data store.
        /// This allows for different persistence strategies (file, database, etc.).
        /// </summary>
        /// <param name="dataStore">The data store to use for persistence.</param>
        /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
        public ProductRepository(IDataStore<Product> dataStore)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        /// <summary>
        /// Gets the in-memory entities.
        /// </summary>
        public IEnumerable<Product> Entities => _entities.AsReadOnly();

        /// <inheritdoc />
        public async Task<Product[]> GetAllAsync()
        {
            await EnsureLoadedAsync();
            return _entities.ToArray();
        }

        /// <inheritdoc />
        public async Task<Product?> GetByIdAsync(int id)
        {
            await EnsureLoadedAsync();
            return _entities.FirstOrDefault(e => e.Id == id);
        }

        /// <inheritdoc />
        public async Task<OperationResult> AddAsync(Product entity)
        {
            if (entity == null)
                return OperationResult.Fail("Entity cannot be null.");

            await EnsureLoadedAsync();

            // Check for duplicate Id (if Id is already assigned)
            if (entity.Id != 0 && _entities.Any(e => e.Id == entity.Id))
            {
                return OperationResult.Fail($"A product with Id {entity.Id} already exists.");
            }

            if (entity.Id == 0)
            {
                entity.Id = _entities.Count > 0 ? _entities.Max(e => e.Id) + 1 : 1;
            }

            _entities.Add(entity);
            return OperationResult.Ok();
        }

        /// <inheritdoc />
        public async Task<OperationResult> UpdateAsync(Product entity)
        {
            if (entity == null)
                return OperationResult.Fail("Entity cannot be null.");

            await EnsureLoadedAsync();

            var index = _entities.FindIndex(e => e.Id == entity.Id);
            if (index < 0)
            {
                return OperationResult.Fail($"Product with Id {entity.Id} was not found.");
            }

            _entities[index] = entity;
            return OperationResult.Ok();
        }

        /// <inheritdoc />
        public async Task<OperationResult> DeleteAsync(Product entity)
        {
            if (entity == null)
                return OperationResult.Fail("Entity cannot be null.");

            await EnsureLoadedAsync();
            var removed = _entities.RemoveAll(e => e.Id == entity.Id);
            return removed > 0
                ? OperationResult.Ok()
                : OperationResult.Fail($"Product with Id {entity.Id} was not found.");
        }

        /// <summary>
        /// Saves all changes to the data store.
        /// </summary>
        /// <returns>The number of entities saved.</returns>
        internal async Task<int> SaveAsync()
        {
            return await _dataStore.SaveAsync(_entities);
        }

        /// <summary>
        /// Reloads data from the data store, discarding any unsaved changes.
        /// </summary>
        public async Task ReloadAsync()
        {
            _entities = new List<Product>(await _dataStore.LoadAsync());
            _isLoaded = true;
        }

        private async Task EnsureLoadedAsync()
        {
            if (!_isLoaded)
            {
                await ReloadAsync();
            }
        }
    }
}
