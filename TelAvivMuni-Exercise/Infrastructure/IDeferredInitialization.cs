namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// Interface for ViewModels that support deferred initialization.
    /// Used in View-First scenarios where the View is created and rendered first,
    /// then data is loaded after the View is ready.
    /// </summary>
    public interface IDeferredInitialization
    {
        /// <summary>
        /// Initializes the ViewModel by loading data.
        /// Call this after the View is fully rendered.
        /// </summary>
        void Initialize();
    }
}
