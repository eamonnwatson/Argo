# EF Core Migrations

This project uses real Entity Framework Core migrations to manage the SQLite database
schema for `ArgoDbContext`. `Database.EnsureCreated()` is **not** used and must not be
reintroduced, since it is incompatible with migrations.

## Applying migrations

Migrations are applied automatically at application startup via
`WebApplicationBuilderExtensions.InitializeArgoDatabase()` (in Argo.Web), which delegates to
`PersistenceServiceExtensions.InitializeArgoDatabase()` and calls `Database.Migrate()`.

## Adding a new migration

Whenever the EF Core model changes (new entities, properties, relationships, etc.),
generate a new migration from the repository root:

```powershell
dotnet ef migrations add <MigrationName> `
  --project src/Argo.Persistence/Argo.Persistence.csproj `
  --startup-project src/Argo.Persistence/Argo.Persistence.csproj `
  --output-dir Migrations
```

A design-time `ArgoDbContextFactory` in this project allows the EF Core tools to
construct an `ArgoDbContext` without starting the full Argo.Web host.
