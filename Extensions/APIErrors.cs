using FluentResults;

namespace Argo.Extensions;

/// <summary>
/// Provides standardized <see cref="IError"/> instances used to map service-layer
/// failures to HTTP responses.
/// </summary>
public static class APIErrors
{
    /// <summary>
    /// Gets an error instance indicating the current caller is not authorized.
    /// </summary>
    public static IError UnauthorizedError => new Error("Unauthorized").WithMetadata("Unauthorized", true);

    /// <summary>
    /// Creates an error instance indicating a requested resource could not be found.
    /// </summary>
    /// <param name="message">The not-found error message to expose to the API caller.</param>
    /// <returns>An <see cref="IError"/> tagged with not-found metadata.</returns>
    public static IError NotFoundError(string message) => new Error(message).WithMetadata("NotFound", true);
}
