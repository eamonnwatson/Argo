namespace Argo.Models;

/// <summary>
/// Represents a user identity available for project ownership and assignment.
/// </summary>
public class User
{
    /// <summary>
    /// Gets the unique domain identifier used as the primary key.
    /// </summary>
    public required string DomainID { get; init; }

    /// <summary>
    /// Gets the display name presented in user-facing lists.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user is eligible for project manager assignment.
    /// </summary>
    public bool IsProjectManager { get; init; }
}
