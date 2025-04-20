using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry;
using Hikkaba.Application.Telemetry.Metrics;
using Hikkaba.Infrastructure.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Hikkaba.Application.Implementations;

// This class is used by the application to send Email and SMS
// when you turn on two-factor authentication in ASP.NET Identity.
// For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
public sealed class AuthMessageSender : IAuthMessageSender
{
    private readonly SmtpClientConfiguration _smtpClientConfiguration;
    private readonly AuthMessageSenderMetrics _authMessageSenderMetrics;

    public AuthMessageSender(
        IOptions<SmtpClientConfiguration> smtpClientConfiguration,
        AuthMessageSenderMetrics authMessageSenderMetrics)
    {
        _smtpClientConfiguration = smtpClientConfiguration.Value;
        _authMessageSenderMetrics = authMessageSenderMetrics;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationTelemetry.AuthMessageSenderSource.StartActivity();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpClientConfiguration.Host, _smtpClientConfiguration.Port, _smtpClientConfiguration.UseSecureConnection ? SecureSocketOptions.Auto : SecureSocketOptions.None, cancellationToken);

        try
        {
            await client.AuthenticateAsync(_smtpClientConfiguration.Username, _smtpClientConfiguration.Password, cancellationToken);

            using var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpClientConfiguration.DisplayName, _smtpClientConfiguration.Username));
            message.To.Add(new MailboxAddress("user", email));
            message.Subject = subject;
            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody,
            };

            message.Body = builder.ToMessageBody();

            await client.SendAsync(message, cancellationToken);

            _authMessageSenderMetrics.EmailSent();
        }
        finally
        {
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
