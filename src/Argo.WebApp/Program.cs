using Argo.Application;
using Argo.Persistence;
using Argo.WebApp.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Direct in-process access to Argo application/persistence services (no HTTP hop to Argo.Web).
// Authentication is intentionally not wired up here; see plan follow-up task.
var dbPath = builder.Configuration.GetConnectionString("ArgoDb") ?? Path.Combine(AppContext.BaseDirectory, "argo.db");

builder.Services.AddArgoPersistence($"Data Source={dbPath}");
builder.Services.AddArgoAppliction();

var app = builder.Build();

// The database is initialized during startup so the app can run without a separate
// migration/bootstrap step for the SQLite store.
app.Services.InitializeArgoDatabase();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
