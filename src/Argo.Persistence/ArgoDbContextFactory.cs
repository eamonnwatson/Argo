using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Argo.Persistence;

/// <summary>
/// Enables EF Core design-time tooling (migrations) to construct an <see cref="ArgoDbContext"/>
/// without requiring the full application host to start.
/// </summary>
internal class ArgoDbContextFactory : IDesignTimeDbContextFactory<ArgoDbContext>
{
    ArgoDbContext IDesignTimeDbContextFactory<ArgoDbContext>.CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArgoDbContext>();
        optionsBuilder.UseSqlite("Data Source=argo.db");

        return new ArgoDbContext(optionsBuilder.Options);
    }
}
