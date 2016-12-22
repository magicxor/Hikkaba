using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CodeKicker.BBCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

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
                    new BBTag("spoiler", "<span class=\"censored\">", "</span>"),
                    new BBTag("quote", "<span class=\"text-success\">", "</span>"),
                });

        public MessagePostProcessor(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        // todo: precompiled regex
        private string UriToHtmlLinks(string text)
        {
            return Regex.Replace(text,
                @"(https?|ftp)(:\/\/[^:(),!{}""\[\]\t\r\n\v\s]+)(\s|$|&#xD|&#xA;|(https?|ftp)(:\/\/))",
                @"<a href=""$1$2"">$1$2</a>");
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

        public string Process(string categoryAlias, Guid threadId, string text)
        {
            var bbParsed = _bbCodeParser.ToHtml(text);
            var uriParsed = UriToHtmlLinks(bbParsed);
            var crossLinksParsed = CrossLinksToHtmlLinks(categoryAlias, threadId, uriParsed);

            var result = crossLinksParsed;
            return result;
        }
    }
}
