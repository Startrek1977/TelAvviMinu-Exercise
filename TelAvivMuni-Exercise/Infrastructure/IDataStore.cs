using System.Threading.Tasks;

namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// Defines a contract for data persistence operations.
    /// Implementations can be file-based, database-based, or any other storage mechanism.
    /// </summary>
    /// <typeparam name="T">The type of entity to store.</typeparam>
    public interface IDataStore<T> where T : class, IEntity
    {
        /// <summary>
        /// Loads all entities from the data store.
        /// </summary>
        /// <returns>An array of entities, or empty array if none exist.</returns>
        Task<T[]> LoadAsync();

        /// <summary>
        /// Saves all entities to the data store.
        /// </summary>
        /// <param name="entities">The entities to save.</param>
        /// <returns>The number of entities saved.</returns>
        Task<int> SaveAsync(IEnumerable<T> entities);
    }
}
