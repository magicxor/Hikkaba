using System.Text.RegularExpressions;
using CodeKicker.BBCode;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Services;

public interface IMessagePostProcessor
{
    string Process(string categoryAlias, TPrimaryKey threadId, string text);
}

public class MessagePostProcessor : IMessagePostProcessor
{
    private readonly IUrlHelper _urlHelper;

    private readonly BbCodeParser _bbCodeParser = new BbCodeParser(new[]
    {
        new BbTag("b", "<b>", "</b>"),
        new BbTag("i", "<i>", "</i>"),
        new BbTag("u", "<u>", "</u>"),
        new BbTag("s", "<s>", "</s>"),
        new BbTag("pre", "<pre>", "</pre>"){StopProcessing = true},
        new BbTag("sub", "<sub>", "</sub>"),
        new BbTag("sup", "<sup>", "</sup>"),
        new BbTag("spoiler", "<span class=\"censored\">", "</span>"),
        new BbTag("quote", "<span class=\"text-success\">&gt; ", "</span>"),
    });

    private static readonly Regex UriRegex = new Regex(@"(((https?|ftp):)(//([^\s/?#]*))?([^\s?#]*)(\?([^\s#]*))?(#([^\s]*))?)", RegexOptions.Compiled);
    private static readonly Regex CrossLinkRegex = new Regex(@"&gt;&gt;([a-z0-9\-]+)", RegexOptions.Compiled);
    private static readonly Regex ReplaceLineTerminatorsRegex = new Regex(@"\u000D\u000A|\u000A|\u000B|\u000C|\u000D|\u0085|\u2028|\u2029", RegexOptions.Compiled);
    private static readonly Regex LimitLineTerminatorCountRegex = new Regex(@"(\u000D\u000A){3,}", RegexOptions.Compiled);

    public MessagePostProcessor(IUrlHelperFactoryWrapper urlHelperFactoryWrapper)
    {
        _urlHelper = urlHelperFactoryWrapper.GetUrlHelper();
    }

    private string ReplaceUrisWithHtmlLinks(string text)
    {
        return UriRegex.Replace(text, @"<a href=""$1"" rel=""nofollow noopener noreferrer external"">$1</a>");
    }

    private string ReplaceCrossLinksWithHtmlLinks(string categoryAlias, TPrimaryKey threadId, string text)
    {
        var threadUri = _urlHelper.Action("Details", "Threads",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            });
        return CrossLinkRegex.Replace(text, @"<a href=""" + threadUri + "#$1" + @""">&gt;&gt;$1</a>");
    }

    private string NormalizeLineBreaks(string text)
    {
        return ReplaceLineTerminatorsRegex.Replace(text, "\r\n");
    }

    private string LimitLineBreaksCount(string text)
    {
        return LimitLineTerminatorCountRegex.Replace(text, "\r\n\r\n");
    }

    public string Process(string categoryAlias, TPrimaryKey threadId, string text)
    {
        var normalizedLineBreaks = NormalizeLineBreaks(text);
        var limitedLineBreaksCount = LimitLineBreaksCount(normalizedLineBreaks);
        var convertedToHtml = _bbCodeParser.ToHtml(limitedLineBreaksCount);
        var linksProcessed = ReplaceUrisWithHtmlLinks(convertedToHtml);
        var crossLinksProcessed = ReplaceCrossLinksWithHtmlLinks(categoryAlias, threadId, linksProcessed);
        return crossLinksProcessed;
    }
}
