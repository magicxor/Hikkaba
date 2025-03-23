using System.Threading.Tasks;

namespace Hikkaba.Services.Contracts;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message);
}