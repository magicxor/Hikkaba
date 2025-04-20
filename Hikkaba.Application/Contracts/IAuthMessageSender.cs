namespace Hikkaba.Application.Contracts;

public interface IAuthMessageSender
{
    Task SendEmailAsync(string email, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
