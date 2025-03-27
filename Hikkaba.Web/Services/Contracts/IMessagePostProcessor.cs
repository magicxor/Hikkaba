namespace Hikkaba.Web.Services.Contracts;

public interface IMessagePostProcessor
{
    string MessageToSafeHtml(string categoryAlias, long threadId, string text);
    string MessageToPlainText(string text);
}
