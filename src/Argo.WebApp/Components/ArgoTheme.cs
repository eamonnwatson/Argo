using MudBlazor;

namespace Argo.WebApp.Components;

/// <summary>
/// Provides the single light-mode MudBlazor theme that reproduces the color palette
/// used by the original Argo.Web static HTML/CSS (see Argo.Web/wwwroot/css/common.css).
/// </summary>
public static class ArgoTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#197f85",
            PrimaryContrastText = "#ffffff",
            Secondary = "#4f86e8",
            Tertiary = "#f6c56c",
            Info = "#4f86e8",
            Success = "#15803d",
            Warning = "#f59e0b",
            Error = "#dc2626",
            Background = "#f3f6fa",
            Surface = "#ffffff",
            AppbarBackground = "#0f172a",
            AppbarText = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "#0f172a",
            DrawerIcon = "#0f172a",
            TextPrimary = "#0f172a",
            TextSecondary = "#64748b",
            LinesDefault = "#e2e8f0",
            TableLines = "#e2e8f0",
            Divider = "#e2e8f0",
            ActionDefault = "#64748b",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
        },
    };
}
