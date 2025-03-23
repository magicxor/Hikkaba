namespace Hikkaba.Web.Services.Contracts;

public interface IMessagePostProcessor
{
    string Process(string categoryAlias, long threadId, string text);
}
