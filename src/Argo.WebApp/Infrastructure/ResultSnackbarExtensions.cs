using FluentResults;
using MudBlazor;

namespace Argo.WebApp.Infrastructure;

public static class ResultSnackbarExtensions
{
    public static bool ShowErrorsIfFailed(this IResultBase result, ISnackbar snackbar)
    {
        if (result.IsSuccess)
            return false;

        foreach (var error in result.Errors)
            snackbar.Add(error.Message, Severity.Error);

        return true;
    }

}
