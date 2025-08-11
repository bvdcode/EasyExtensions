namespace EasyExtensions.EntityFrameworkCore.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an entity type is invalid.
    /// </summary>
    public class EntityInvalidTypeException(string message) : Exception(message)
    {
        /// <summary>
        /// Throws an exception indicating that the entity type is invalid.
        /// </summary>
        /// <exception cref="EntityInvalidTypeException">Thrown when the entity type is invalid.</exception>
        public static TEntity ThrowIfInvalidType<TEntity>(object? value, string? paramName = null)
        {
            if (value is not TEntity typed)
            {
                throw new EntityInvalidTypeException(
                    $"Expected {typeof(TEntity).FullName}, got {value?.GetType().FullName ?? "null"}"
                    + (paramName is null ? string.Empty : $" for parameter '{paramName}'."));
            }
            return typed;
        }
    }
}
