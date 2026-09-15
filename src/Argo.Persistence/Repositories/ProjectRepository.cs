using Argo.Application.Repositories;
using Argo.Domain.Entities;
using Argo.Domain.ValueObjects;
using Argo.Persistence.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Argo.Persistence.Repositories;

internal class ProjectRepository(ArgoDbContext dbContext) : BaseRepository, IProjectRepository
{
    private readonly ArgoDbContext dbContext = dbContext;

    public Task<Result<Project?>> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken));

    public Task<Result<Project?>> GetByIdWithDetailsAsync(ProjectId id, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.Projects
            .Include(p => p.Owner)
            .Include(p => p.WorkItems)
                .ThenInclude(w => w.Activities)
            .Include(p => p.RaidItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken));

    public Task<Result<IReadOnlyCollection<Project>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<Project>)await dbContext.Projects.ToListAsync(cancellationToken));

    public Task<Result<IReadOnlyCollection<Project>>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(async () => (IReadOnlyCollection<Project>)await dbContext.Projects.AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.WorkItems)
            .ThenInclude(w => w.Activities)
            .Include(p => p.RaidItems)
            .ToListAsync(cancellationToken));

    public Task<Result<bool>> ExistsBySourceRequestIdAsync(string sourceRequestId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.Projects.AnyAsync(p => p.SourceRequestId == sourceRequestId, cancellationToken));

    public void Add(Project entity) => dbContext.Projects.Add(entity);

    public void Remove(Project entity) => dbContext.Projects.Remove(entity);

    public Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => dbContext.SaveChangesAsync(cancellationToken));
}
