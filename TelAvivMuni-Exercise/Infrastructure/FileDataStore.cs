using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// File-based implementation of IDataStore.
    /// Uses an ISerializer for format flexibility (JSON, XML, etc.).
    /// Thread-safe for async operations.
    /// </summary>
    /// <typeparam name="T">The type of entity to store.</typeparam>
    public class FileDataStore<T> : IDataStore<T> where T : class, IEntity
    {
        private readonly string _filePath;
        private readonly ISerializer<T> _serializer;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the FileDataStore.
        /// </summary>
        /// <param name="filePath">The path to the data file.</param>
        /// <param name="serializer">The serializer to use for reading/writing data.</param>
        /// <exception cref="ArgumentNullException">Thrown when filePath or serializer is null.</exception>
        public FileDataStore(string filePath, ISerializer<T> serializer)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <summary>
        /// Gets the file path being used by this data store.
        /// </summary>
        public string FilePath => _filePath;

        /// <inheritdoc />
        public async Task<T[]> LoadAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    return Array.Empty<T>();
                }

                var content = await File.ReadAllTextAsync(_filePath);
                return await _serializer.DeserializeAsync(content);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<int> SaveAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await _semaphore.WaitAsync();
            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var entityArray = entities as T[] ?? entities.ToArray();
                var content = await _serializer.SerializeAsync(entityArray);
                await File.WriteAllTextAsync(_filePath, content);

                return entityArray.Length;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
