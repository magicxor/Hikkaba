namespace Hikkaba.Application.Contracts;

public interface ISmsSender
{
    Task SendSmsAsync(string number, string message);
}