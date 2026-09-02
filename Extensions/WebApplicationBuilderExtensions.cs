using Argo.Data;
using Argo.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore;

namespace Argo.Extensions;

/// <summary>
/// Provides startup extension methods that register Argo application services
/// and prepare the backing SQLite database at application boot.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Registers framework and Argo-specific services required by the web application.
    /// </summary>
    /// <param name="builder">The application builder used to configure dependency injection and host services.</param>
    /// <returns>The same <see cref="WebApplicationBuilder"/> instance to allow fluent startup configuration.</returns>
    /// <remarks>
    /// When no <c>ArgoDb</c> connection string is configured, the application falls back
    /// to a SQLite file located in the host base directory.
    /// </remarks>
    public static WebApplicationBuilder AddArgoServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddWindowsService(options => options.ServiceName = "Argo WebService");

        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails();
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
            options.AddPolicy("ArgoUser", policy => policy.RequireAuthenticatedUser().AddRequirements(new ArgoUserRequirement()));
        });

        builder.Services.AddScoped<IAuthorizationHandler, ArgoUserAuthorizationHandler>();
        builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ArgoAuthorizationResultHandler>();

        builder.Services.AddHttpContextAccessor();

        var dbPath = builder.Configuration.GetConnectionString("ArgoDb") ?? Path.Combine(AppContext.BaseDirectory, "argo.db");

        builder.Services.AddDbContext<ArgoDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddTransient<IArgoService, ArgoService>();

        return builder;
    }

    /// <summary>
    /// Ensures the application's SQLite database is created and configured for write-ahead logging.
    /// </summary>
    /// <param name="app">The application instance providing access to registered services.</param>
    /// <returns>The same <see cref="WebApplication"/> instance to continue pipeline configuration.</returns>
    /// <remarks>
    /// The database is initialized during startup so deployments do not require a separate
    /// manual database bootstrap step.
    /// </remarks>
    public static WebApplication InitializeArgoDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ArgoDbContext>();
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

        return app;
    }
}
