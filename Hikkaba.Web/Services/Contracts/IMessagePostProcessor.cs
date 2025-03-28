using System.Collections.Generic;

namespace Hikkaba.Web.Services.Contracts;

public interface IMessagePostProcessor
{
    string MessageToSafeHtml(string categoryAlias, long? threadId, string text);
    string MessageToPlainText(string text);
    public IReadOnlyList<long> GetMentionedPosts(string text);
}
