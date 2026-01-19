using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// JSON implementation of the serializer using System.Text.Json.
    /// Uses stream-based async operations for true non-blocking I/O.
    /// </summary>
    /// <typeparam name="T">The type of entity to serialize.</typeparam>
    public class JsonSerializer<T> : ISerializer<T> where T : class
    {
        private readonly JsonSerializerOptions? _options;

        /// <summary>
        /// Initializes a new instance of the JsonSerializer with default options.
        /// </summary>
        public JsonSerializer()
            : this(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonSerializer with custom options.
        /// </summary>
        /// <param name="options">The JSON serializer options, or null to use System.Text.Json defaults.</param>
        public JsonSerializer(JsonSerializerOptions? options)
        {
            _options = options;
        }

        /// <inheritdoc />
        public string FileExtension => ".json";

        /// <inheritdoc />
        public async Task<string> SerializeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            using (var stream = new MemoryStream())
            {
                await System.Text.Json.JsonSerializer.SerializeAsync(stream, entities, _options);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        /// <inheritdoc />
        public async Task<T[]> DeserializeAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Array.Empty<T>();
            }

            try
            {
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
                {
                    var entities = await System.Text.Json.JsonSerializer.DeserializeAsync<T[]>(stream, _options);
                    return entities ?? Array.Empty<T>();
                }
            }
            catch (JsonException)
            {
                return Array.Empty<T>();
            }
        }
    }
}
