using TPrimaryKey = System.Guid;
using System.Text.RegularExpressions;
using CodeKicker.BBCode;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Services
{
    public interface IMessagePostProcessor
    {
        string Process(string categoryAlias, TPrimaryKey threadId, string text);
    }

    public class MessagePostProcessor : IMessagePostProcessor
    {
        private readonly IUrlHelper _urlHelper;

        private readonly BBCodeParser _bbCodeParser = new BBCodeParser(new[]
                {
                    new BBTag("b", "<b>", "</b>"),
                    new BBTag("i", "<i>", "</i>"),
                    new BBTag("u", "<u>", "</u>"),
                    new BBTag("s", "<s>", "</s>"),
                    new BBTag("pre", "<pre>", "</pre>"){StopProcessing = true},
                    new BBTag("sub", "<sub>", "</sub>"),
                    new BBTag("sup", "<sup>", "</sup>"),
                    new BBTag("spoiler", "<span class=\"censored\">", "</span>"),
                    new BBTag("quote", "<span class=\"text-success\">&gt; ", "</span>"),
                });

        public MessagePostProcessor(IUrlHelperFactoryWrapper urlHelperFactoryWrapper)
        {
            _urlHelper = urlHelperFactoryWrapper.GetUrlHelper();
        }

        // todo: precompiled regex
        private string UriToHtmlLinks(string text)
        {
            return Regex.Replace(text,
                @"(https?|ftp)(:\/\/[^:;(),!{}""\s]+)(\s|$|\&\#x[a-zA-Z0-9]+?;|\&quot;|!|,|\?|\(|\))",
                @"<a href=""$1$2"">$1$2</a>$3");
        }

        private string CrossLinksToHtmlLinks(string categoryAlias, TPrimaryKey threadId, string text)
        {
            var threadUri = Regex.Escape(_urlHelper.Action("Details", "Threads",
                        new
                        {
                            categoryAlias = categoryAlias,
                            threadId = threadId
                        }));
            return Regex.Replace(text,
                @"&gt;&gt;([a-z0-9\-]+)",
                 @"<a href=""" + threadUri + "#$1" + @""">&gt;&gt;$1</a>");
        }

        private string ReplaceLineTerminators(string text)
        {
            return Regex.Replace(text,
                @"\u000D\u000A|\u000A|\u000B|\u000C|\u000D|\u0085|\u2028|\u2029",
                "\r\n");
        }

        private string LimitLineTerminatorCount(string text)
        {
            return Regex.Replace(text, @"(\u000D\u000A){3,}", "\r\n\r\n");
        }

        public string Process(string categoryAlias, TPrimaryKey threadId, string text)
        {
            text = ReplaceLineTerminators(text);
            text = LimitLineTerminatorCount(text);
            var bbParsed = _bbCodeParser.ToHtml(text);
            var uriParsed = UriToHtmlLinks(bbParsed);
            var crossLinksParsed = CrossLinksToHtmlLinks(categoryAlias, threadId, uriParsed);
            var result = crossLinksParsed;
            return result;
        }
    }
}
