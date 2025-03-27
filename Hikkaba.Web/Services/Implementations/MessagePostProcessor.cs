using System.Collections.Generic;
using System.Text.RegularExpressions;
using BBCodeParser;
using BBCodeParser.Tags;
using Hikkaba.Services.Implementations;
using Hikkaba.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Services.Implementations;

public partial class MessagePostProcessor : IMessagePostProcessor
{
    private readonly IUrlHelper _urlHelper;

    private readonly BbParser _bbParser = new BbParser([
        new Tag("b", "<b>", "</b>"),
        new Tag("i", "<i>", "</i>"),
        new Tag("u", "<u>", "</u>"),
        new Tag("s", "<s>", "</s>"),
        new PreformattedTag("pre", "<pre>", "</pre>"),
        new Tag("sub", "<sub>", "</sub>"),
        new Tag("sup", "<sup>", "</sup>"),
        new Tag("spoiler", """<span class="censored">""", "</span>"),
        new Tag("quote", """<span class="text-success">&gt; """, "</span>"),
        new Tag("url", """<a href="{value}" rel="nofollow noopener noreferrer external">""", "</a>", withAttribute: true, attributeEscape: AttributeEscapeMode.AbsoluteUri),
        new Tag("relurl", """<a href="{value}">""", "</a>", withAttribute: true, attributeEscape: AttributeEscapeMode.RelativeUri),
    ], BbParser.SecuritySubstitutions, new Dictionary<string, string>());

    [GeneratedRegex("""(\b(?<protocol>https?|ftp)://(?<domain>[-\p{L}\p{M}\p{N}.]+)(?<port>:[0-9]+)?(?<file>/[-A-Z0-9+&@#/%=~_|!:,.;]*)?(?<parameters>\?[-A-Z0-9+&@#/%=~_|!:,.;]*)?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, 500)]
    public static partial Regex UriRegexClass();

    [GeneratedRegex(@">>([a-z0-9\-]+)", RegexOptions.Compiled, 500)]
    private static partial Regex CrossLinkRegexClass();

    [GeneratedRegex(@"\u000D\u000A|\u000A|\u000B|\u000C|\u000D|\u0085|\u2028|\u2029", RegexOptions.Compiled, 500)]
    private static partial Regex ReplaceLineTerminatorsRegexClass();

    [GeneratedRegex(@"(\u000D\u000A){3,}", RegexOptions.Compiled, 500)]
    private static partial Regex LimitLineTerminatorCountRegexClass();

    private static readonly Regex UriRegex = UriRegexClass();
    private static readonly Regex CrossLinkRegex = CrossLinkRegexClass();
    private static readonly Regex ReplaceLineTerminatorsRegex = ReplaceLineTerminatorsRegexClass();
    private static readonly Regex LimitLineTerminatorCountRegex = LimitLineTerminatorCountRegexClass();

    public MessagePostProcessor(
        IUrlHelperFactoryWrapper urlHelperFactoryWrapper)
    {
        _urlHelper = urlHelperFactoryWrapper.GetUrlHelper();
    }

    private static string ReplaceUrisWithHtmlLinks(string text)
    {
        return UriRegex.Replace(text, """<a href="$1" rel="nofollow noopener noreferrer external">$1</a>""");
    }

    private string ReplaceCrossLinksWithHtmlLinks(string categoryAlias, long threadId, string text)
    {
        var threadUri = _urlHelper.Action("Details", "Threads",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            });
        return CrossLinkRegex.Replace(text, $"""<a href="{threadUri}#$1">&gt;&gt;$1</a>""");
    }

    private static string ReplaceUrisWithBbCodeUrl(string text)
    {
        return UriRegex.Replace(text, """[url="$1"]$1[/url]""");
    }

    private string ReplaceCrossLinksWithBbCodeUrl(string categoryAlias, long threadId, string text)
    {
        var threadUri = _urlHelper.Action("Details", "Threads",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            });
        return CrossLinkRegex.Replace(text, $"""[relurl="{threadUri}#$1"]>>$1[/relurl]""");
    }

    private static string NormalizeLineBreaks(string text)
    {
        return ReplaceLineTerminatorsRegex.Replace(text, "\r\n");
    }

    private static string LimitLineBreaksCount(string text)
    {
        return LimitLineTerminatorCountRegex.Replace(text, "\r\n\r\n");
    }

    public string MessageToSafeHtml(string categoryAlias, long threadId, string text)
    {
        var normalizedLineBreaks = NormalizeLineBreaks(text);
        var limitedLineBreaksCount = LimitLineBreaksCount(normalizedLineBreaks);
        var linksProcessed = ReplaceUrisWithBbCodeUrl(limitedLineBreaksCount);
        var crossLinksProcessed = ReplaceCrossLinksWithBbCodeUrl(categoryAlias, threadId, linksProcessed);
        var convertedToHtml = _bbParser.Parse(crossLinksProcessed).ToHtml();
        return convertedToHtml;
    }

    public string MessageToPlainText(string text)
    {
        var convertedBbToHtml = _bbParser.Parse(text).ToHtml();
        var extractedPlainText = HtmlUtilities.ConvertToPlainText(convertedBbToHtml);
        var normalizedLineBreaks = NormalizeLineBreaks(extractedPlainText);
        var limitedLineBreaksCount = LimitLineBreaksCount(normalizedLineBreaks);
        return limitedLineBreaksCount;
    }
}
