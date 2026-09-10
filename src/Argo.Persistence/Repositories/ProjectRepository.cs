using Argo.Application.Repositories;
using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Argo.Persistence.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IProjectRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
internal class ProjectRepository(ArgoDbContext dbContext) : BaseRepository, IProjectRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public Task<Result<Project?>> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken));

    /// <inheritdoc />
    public Task<Result<Project?>> GetByIdWithDetailsAsync(ProjectId id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.Projects
            .Include(p => p.Owner)
            .Include(p => p.WorkItems)
                .ThenInclude(w => w.Activities)
            .Include(p => p.RaidItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<Project>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<Project>)await dbContext.Projects.ToListAsync(cancellationToken));

    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<Project>>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<Project>)await dbContext.Projects.AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.WorkItems)
            .ThenInclude(w => w.Activities)
            .Include(p => p.RaidItems)
            .ToListAsync(cancellationToken));

    /// <inheritdoc />
    public Task<Result<bool>> ExistsBySourceRequestIdAsync(string sourceRequestId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.Projects.AnyAsync(p => p.SourceRequestId == sourceRequestId, cancellationToken));

    /// <inheritdoc />
    public void Add(Project entity) => dbContext.Projects.Add(entity);

    /// <inheritdoc />
    public void Remove(Project entity) => dbContext.Projects.Remove(entity);

    /// <inheritdoc />
    public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.SaveChangesAsync(cancellationToken));
}
