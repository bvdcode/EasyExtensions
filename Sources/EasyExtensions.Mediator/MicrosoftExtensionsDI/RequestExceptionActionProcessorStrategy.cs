#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Specifies the strategy used to determine when to process actions in response to request exceptions.
    /// </summary>
    /// <remarks>Use this enumeration to control whether exception actions are applied only to unhandled
    /// exceptions or to all exceptions encountered during request processing. This can be useful for customizing error
    /// handling behavior in request pipelines.</remarks>
    public enum RequestExceptionActionProcessorStrategy
    {
        /// <summary>
        /// Gets or sets a value indicating whether the policy should be applied to unhandled exceptions.
        /// </summary>
        ApplyForUnhandledExceptions,

        /// <summary>
        /// Gets or sets a value indicating whether the operation should be applied to all exceptions, regardless of
        /// type.
        /// </summary>
        ApplyForAllExceptions
    }
}