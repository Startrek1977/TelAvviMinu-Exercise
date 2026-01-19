using System.Threading.Tasks;

namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// Defines a contract for serializing and deserializing collections of entities.
    /// </summary>
    /// <typeparam name="T">The type of entity to serialize.</typeparam>
    public interface ISerializer<T> where T : class
    {
        /// <summary>
        /// Gets the file extension associated with this serializer (e.g., ".json", ".xml").
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Serializes a collection of entities to a string.
        /// </summary>
        /// <param name="entities">The entities to serialize.</param>
        /// <returns>The serialized string representation.</returns>
        Task<string> SerializeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Deserializes a string to an array of entities.
        /// </summary>
        /// <param name="content">The serialized content.</param>
        /// <returns>The deserialized array of entities.</returns>
        Task<T[]> DeserializeAsync(string content);
    }
}
