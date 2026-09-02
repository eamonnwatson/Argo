using Argo.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddArgoServices();

var app = builder.Build();

// Centralized exception handling returns a consistent JSON payload for unhandled errors.
// Detailed exception messages are only exposed in development to avoid leaking internals.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var message = app.Environment.IsDevelopment() && exceptionFeature is not null
            ? exceptionFeature.Error.Message
            : "An unexpected error occurred.";

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

// The database is initialized during startup so deployments can run without
// a separate migration/bootstrap step for the SQLite store.
app.InitializeArgoDatabase();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapArgoApi();

app.Run();
