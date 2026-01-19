namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// Represents the result of a repository operation.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the error message if the operation failed, or null if it succeeded.
        /// </summary>
        public string? ErrorMessage { get; }

        private OperationResult(bool success, string? errorMessage = null)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static OperationResult Ok() => new(true);

        /// <summary>
        /// Creates a failed result with an optional error message.
        /// </summary>
        public static OperationResult Fail(string? errorMessage = null) => new(false, errorMessage);

        /// <summary>
        /// Allows implicit conversion to bool for simple success checks.
        /// </summary>
        public static implicit operator bool(OperationResult result) => result.Success;
    }
}
