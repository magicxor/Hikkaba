using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Narochno.BBCode;

namespace Hikkaba.Web.Services
{
    public interface IMessagePostProcessor
    {
        string Process(string categoryAlias, Guid threadId, string text);
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
                    new BBTag("pre", "<pre>", "</pre>"),
                    new BBTag("sub", "<sub>", "</sub>"),
                    new BBTag("sup", "<sup>", "</sup>"),
                    new BBTag("spoiler", "<span class=\"censored\">", "</span>"),
                    new BBTag("quote", "<span class=\"text-success\">&gt; ", "</span>"),
                });

        public MessagePostProcessor(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        // todo: precompiled regex
        private string UriToHtmlLinks(string text)
        {
            return Regex.Replace(text,
                @"(https?|ftp)(:\/\/[^:;(),!{}""\s]+)(\s|$|\&\#x[a-zA-Z0-9]+?;|\&quot;|!|,|\?|\(|\))",
                @"<a href=""$1$2"">$1$2</a>$3");
        }

        private string CrossLinksToHtmlLinks(string categoryAlias, Guid threadId, string text)
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

        private string LineBreaksToHtmlTags(string text)
        {
            return text.Replace("&#xA;", "<br/>").Replace("&#xD;", "");
        }

        public string Process(string categoryAlias, Guid threadId, string text)
        {
            var bbParsed = _bbCodeParser.ToHtml(text);
            var uriParsed = UriToHtmlLinks(bbParsed);
            var crossLinksParsed = CrossLinksToHtmlLinks(categoryAlias, threadId, uriParsed);
            var lineBreaksParsed = LineBreaksToHtmlTags(crossLinksParsed);

            var result = lineBreaksParsed;
            return result;
        }
    }
}
