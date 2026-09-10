using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using FluentResults;

namespace Argo.Application.Repositories;

/// <summary>
/// Provides data access for <see cref="User"/> entities.
/// </summary>
public interface IUserRepository : IRepository<User, UserId>
{
    /// <summary>
    /// Retrieves users ordered by display name, optionally filtered to project managers.
    /// </summary>
    Task<Result<IReadOnlyCollection<User>>> GetAllAsync(bool projectManagersOnly, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by display name.
    /// </summary>
    Task<Result<User?>> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default);
}
