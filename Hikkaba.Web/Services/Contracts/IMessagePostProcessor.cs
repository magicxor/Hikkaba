namespace Hikkaba.Web.Services.Contracts;

public interface IMessagePostProcessor
{
    string Process(string categoryAlias, TPrimaryKey threadId, string text);
}