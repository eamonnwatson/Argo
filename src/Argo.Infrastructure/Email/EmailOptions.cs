namespace Argo.Infrastructure.Email;

/// <summary>
/// Configuration options for outbound SMTP email delivery.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets or sets the SMTP host name.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP port.
    /// </summary>
    public int Port { get; set; } = 25;

    /// <summary>
    /// Gets or sets the email address used as the message sender.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name used as the message sender.
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the SMTP connection should use SSL/TLS.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Gets or sets the SMTP authentication username, if required.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the SMTP authentication password, if required.
    /// </summary>
    public string? Password { get; set; }
}
