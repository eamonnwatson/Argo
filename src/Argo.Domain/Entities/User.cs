using Argo.Domain.Common;
using Argo.Domain.ValueObjects;
using FluentResults;

namespace Argo.Domain.Entities;

public class User : Entity<UserId>
{
    private User(UserId id)
    {
        Id = id;
    }

    public string DisplayName { get; internal set; } = string.Empty;
    public bool IsProjectManager { get; internal set; }
    public string? Email { get; internal set; }

    public static Result<User> Create(UserId id, string displayName, bool isProjectManager, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Fail<User>(DomainErrors.Required(nameof(DisplayName)));

        if (email is not null && string.IsNullOrWhiteSpace(email))
            return Result.Fail<User>(DomainErrors.Required(nameof(Email)));

        var user = new User(id)
        {
            DisplayName = displayName,
            IsProjectManager = isProjectManager,
            Email = email
        };

        return Result.Ok(user);
    }
}
