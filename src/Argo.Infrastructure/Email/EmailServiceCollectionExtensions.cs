using Argo.Application.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Argo.Infrastructure.Email;

/// <summary>
/// Provides dependency injection registration for the FluentEmail-based
/// <see cref="IEmailSender"/> implementation.
/// </summary>
public static class EmailServiceCollectionExtensions
{
    /// <summary>
    /// Registers FluentEmail with an SMTP sender configured from <paramref name="emailOptions"/>
    /// and registers <see cref="FluentEmailSender"/> as the <see cref="IEmailSender"/> implementation.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="emailOptions">The SMTP delivery options to use.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance to allow fluent configuration.</returns>
    public static IServiceCollection AddArgoEmail(this IServiceCollection services, EmailOptions emailOptions)
    {
        services
            .AddFluentEmail(emailOptions.FromAddress, emailOptions.FromName)
            .AddSmtpSender(() =>
            {
                var smtpClient = new System.Net.Mail.SmtpClient(emailOptions.Host, emailOptions.Port)
                {
                    EnableSsl = emailOptions.UseSsl
                };

                if (!string.IsNullOrWhiteSpace(emailOptions.Username))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(emailOptions.Username, emailOptions.Password);
                }

                return smtpClient;
            });

        services.AddScoped<IEmailSender, FluentEmailSender>();

        return services;
    }
}
