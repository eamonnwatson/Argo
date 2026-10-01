using Argo.Web.Extensions;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Warnings and errors only, written to a rolling file next to the executable
// (a Windows service's working directory is System32, so the path must be absolute).
builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .MinimumLevel.Warning()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "logs", "argo-.log"),
        restrictedToMinimumLevel: LogEventLevel.Warning,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        rollOnFileSizeLimit: true,
        shared: true));

try
{
    builder.AddArgoServices();

    var app = builder.Build();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (exceptionFeature is not null)
            {
                app.Logger.LogError(exceptionFeature.Error, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            var message = app.Environment.IsDevelopment() && exceptionFeature is not null
                ? exceptionFeature.Error.Message
                : "An unexpected error occurred.";

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = message });
        });
    });

    app.InitializeArgoDatabase();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapArgoApi();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Argo terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
