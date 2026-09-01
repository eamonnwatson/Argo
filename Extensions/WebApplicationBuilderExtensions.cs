using Argo.Data;
using Argo.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;

namespace Argo.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddArgoServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddWindowsService(options => options.ServiceName = "Argo WebService");

        builder.Services.AddOpenApi();
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

        builder.Services.AddAuthorization(options => options.FallbackPolicy = options.DefaultPolicy);

        builder.Services.AddHttpContextAccessor();

        var dbPath = builder.Configuration.GetConnectionString("ArgoDb") ?? Path.Combine(AppContext.BaseDirectory, "argo.db");

        builder.Services.AddDbContext<ArgoDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddTransient<IArgoService, ArgoService>();

        return builder;
    }

    public static WebApplication InitializeArgoDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ArgoDbContext>();
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

        return app;
    }
}
