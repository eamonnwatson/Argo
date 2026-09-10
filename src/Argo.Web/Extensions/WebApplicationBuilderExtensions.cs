using Argo.Data;
using Argo.Infrastructure.Email;
using Argo.Notifications;
using Argo.Outbox;
using Argo.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;

namespace Argo.Extensions;

/// <summary>
/// Provides startup extension methods that register Argo application services
/// and prepare the backing SQLite database at application boot.
/// </summary>
/// <remarks>
/// The SQLite database context, repositories, and EF Core migrations now live in the
/// Argo.Persistence project (see <c>src/Argo.Persistence</c>). Schema changes must be introduced via
/// <c>dotnet ef migrations add &lt;Name&gt; --project src/Argo.Persistence/Argo.Persistence.csproj --startup-project src/Argo.Persistence/Argo.Persistence.csproj --output-dir Migrations</c>
/// and are applied at startup by <see cref="InitializeArgoDatabase"/>, which delegates to
/// <c>PersistenceServiceExtensions.InitializeArgoDatabase</c>.
/// Do not reintroduce <c>Database.EnsureCreated()</c>, as it is incompatible with migrations.
/// </remarks>
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

        builder.Services.AddArgoPersistence($"Data Source={dbPath}");

        builder.Services.AddTransient<IArgoService, ArgoService>();

        var emailOptions = builder.Configuration.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions();
        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

        builder.Services
            .AddFluentEmail(emailOptions.FromAddress, emailOptions.FromName)
            .AddSmtpSender(() =>
            {
                var smtpClient = new System.Net.Mail.SmtpClient(emailOptions.Host, emailOptions.Port)
                {
                    EnableSsl = emailOptions.UseSsl
                };

                if (!string.IsNullOrWhiteSpace(emailOptions.Username))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(emailOptions.Username, emailOptions.Password);
                }

                return smtpClient;
            });

        builder.Services.AddScoped<IEmailSender, FluentEmailSender>();
        builder.Services.AddScoped<ProjectManagerChangedNotificationHandler>();
        builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher>();
        builder.Services.AddHostedService<OutboxProcessor>();

        return builder;
    }

    /// <summary>
    /// Applies any pending EF Core migrations to the application's SQLite database and configures
    /// it for write-ahead logging.
    /// </summary>
    /// <param name="app">The application instance providing access to registered services.</param>
    /// <returns>The same <see cref="WebApplication"/> instance to continue pipeline configuration.</returns>
    /// <remarks>
    /// The database is migrated during startup so deployments do not require a separate
    /// manual database bootstrap step. Schema changes must be made via new EF Core migrations
    /// (<c>dotnet ef migrations add &lt;Name&gt;</c>) rather than by relying on EnsureCreated().
    /// </remarks>
    public static WebApplication InitializeArgoDatabase(this WebApplication app)
    {
        app.Services.InitializeArgoDatabase();

        return app;
    }
}
