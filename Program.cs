using Argo.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddArgoServices();

var app = builder.Build();

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

app.InitializeArgoDatabase();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapArgoApi();

app.Run();
