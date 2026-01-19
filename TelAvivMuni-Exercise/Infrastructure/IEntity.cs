namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// Defines a contract for entities with an identifier.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        int Id { get; set; }
    }
}
