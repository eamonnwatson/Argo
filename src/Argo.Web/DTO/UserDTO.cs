namespace Argo.DTO;

/// <summary>
/// Represents a user record returned for assignment and ownership selection.
/// </summary>
/// <param name="DomainID">The unique domain identifier.</param>
/// <param name="DisplayName">The display name shown in the UI.</param>
/// <param name="IsProjectManager">Indicates whether the user can be selected as a project manager.</param>
public record UserDTO(string DomainID, string DisplayName, bool IsProjectManager);
