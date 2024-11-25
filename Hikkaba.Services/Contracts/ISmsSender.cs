using System.Threading.Tasks;

namespace Hikkaba.Services.Contracts;

public interface ISmsSender
{
    Task SendSmsAsync(string number, string message);
}