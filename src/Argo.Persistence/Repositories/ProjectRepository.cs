using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Argo.Data.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IProjectRepository"/>.
/// </summary>
/// <param name="dbContext">The EF Core context used for persistence operations.</param>
public class ProjectRepository(ArgoDbContext dbContext) : IProjectRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default) =>
        await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<Project?> GetByIdWithDetailsAsync(ProjectId id, CancellationToken cancellationToken = default) =>
        await dbContext.Projects
            .Include(p => p.Owner)
            .Include(p => p.WorkItems)
                .ThenInclude(w => w.Activities)
            .Include(p => p.RaidItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Projects.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Project>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Projects.AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.WorkItems)
                .ThenInclude(w => w.Activities)
            .Include(p => p.RaidItems)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public void Add(Project entity) => dbContext.Projects.Add(entity);

    /// <inheritdoc />
    public void Remove(Project entity) => dbContext.Projects.Remove(entity);

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
