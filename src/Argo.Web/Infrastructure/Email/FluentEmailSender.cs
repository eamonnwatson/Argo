using Argo.Notifications;
using FluentEmail.Core;

namespace Argo.Infrastructure.Email;

/// <summary>
/// Sends notification emails using the FluentEmail library configured with the
/// application's SMTP settings.
/// </summary>
/// <param name="fluentEmail">The configured FluentEmail sender.</param>
public class FluentEmailSender(IFluentEmail fluentEmail) : IEmailSender
{
    private readonly IFluentEmail fluentEmail = fluentEmail;

    /// <inheritdoc />
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        await fluentEmail
            .To(to)
            .Subject(subject)
            .Body(body)
            .SendAsync(cancellationToken);
    }
}
