namespace Argo.Application.Notifications;

/// <summary>
/// Provides an abstraction for sending notification emails, decoupling handlers from
/// the underlying email delivery implementation.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The email body content.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
