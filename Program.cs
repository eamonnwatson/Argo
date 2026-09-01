using Argo.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddArgoServices();

var app = builder.Build();

app.InitializeArgoDatabase();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapArgoApi();

app.Run();
