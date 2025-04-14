using System.Collections.Generic;
using System.Text.RegularExpressions;
using BBCodeParser;
using BBCodeParser.Enums;
using BBCodeParser.Tags;
using Hikkaba.Application.Implementations;
using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.Telemetry;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Services.Implementations;

public partial class MessagePostProcessor : IMessagePostProcessor
{
    private readonly IUrlHelper _urlHelper;

    private readonly BBParser _bbParser = new([
        new Tag("b", "<b>", "</b>"),
        new Tag("i", "<i>", "</i>"),
        new Tag("u", "<u>", "</u>"),
        new Tag("s", "<s>", "</s>"),
        new CodeTag("code", """<pre class="code" data-syntax="{value}">""", "</pre>"),
        new Tag("sub", "<sub>", "</sub>"),
        new Tag("sup", "<sup>", "</sup>"),
        new Tag("spoiler", """<span class="censored">""", "</span>"),
        new Tag("quote", """<span class="text-success">&gt; """, "</span>"),
        new Tag("url", """<a href="{value}" rel="nofollow noopener noreferrer external">""", "</a>", withAttribute: true, attributeEscape: AttributeEscapeMode.AbsoluteUri),
        new Tag("relurl", """<a href="{value}">""", "</a>", withAttribute: true, attributeEscape: AttributeEscapeMode.RelativeUri),
    ], BBParser.SecuritySubstitutions, new Dictionary<string, string>());

    [GeneratedRegex("""(\b(?<protocol>https?|ftp)://(?<domain>[-\p{L}\p{M}\p{N}.]+)(?<port>:[0-9]+)?(?<file>/[-A-Z0-9+&@#/%=~_|!:,.;]*)?(?<parameters>\?[-A-Z0-9+&@#/%=~_|!:,.;]*)?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, 500)]
    public static partial Regex GetUriRegex();

    [GeneratedRegex(@"\u000D\u000A|\u000A|\u000B|\u000C|\u000D|\u0085|\u2028|\u2029", RegexOptions.Compiled, 500)]
    private static partial Regex GetReplaceLineTerminatorsRegex();

    [GeneratedRegex(@"(\u000D\u000A|\u000A){3,}", RegexOptions.Compiled, 500)]
    private static partial Regex GetLimitLineTerminatorCountRegex();

    [GeneratedRegex(@">>([0-9]+)", RegexOptions.Compiled, 500)]
    private static partial Regex GetPostLinkRegex();

    private static readonly Regex UriRegex = GetUriRegex();
    private static readonly Regex PostLinkRegex = GetPostLinkRegex();
    private static readonly Regex ReplaceLineTerminatorsRegex = GetReplaceLineTerminatorsRegex();
    private static readonly Regex LimitLineTerminatorCountRegex = GetLimitLineTerminatorCountRegex();

    public MessagePostProcessor(
        IUrlHelperFactoryWrapper urlHelperFactoryWrapper)
    {
        _urlHelper = urlHelperFactoryWrapper.GetUrlHelper();
    }

    private static string ReplaceUrisWithBbCodeUrl(string text)
    {
        using var activity = WebTelemetry.MessagePostProcessorSource.StartActivity();
        return UriRegex.Replace(text, """[url="$1"]$1[/url]""");
    }

    private string ReplacePostLinksWithBbCodeUrl(string categoryAlias, long threadId, string text)
    {
        using var activity = WebTelemetry.MessagePostProcessorSource.StartActivity();
        var threadUri = _urlHelper.RouteUrl(
            "ThreadDetails",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            });
        return PostLinkRegex.Replace(text, $"""[relurl="{threadUri}#$1"]>>$1[/relurl]""");
    }

    private static string NormalizeLineBreaks(string text)
    {
        using var activity = WebTelemetry.MessagePostProcessorSource.StartActivity();
        return ReplaceLineTerminatorsRegex.Replace(text, "\n");
    }

    private static string LimitLineBreaksCount(string text)
    {
        using var activity = WebTelemetry.MessagePostProcessorSource.StartActivity();
        return LimitLineTerminatorCountRegex.Replace(text, "\n\n");
    }

    public string MessageToSafeHtml(string categoryAlias, long? threadId, string text)
    {
        using var activity = WebTelemetry.MessagePostProcessorSource.StartActivity();
        var normalizedLineBreaks = NormalizeLineBreaks(text);
        var limitedLineBreaksCount = LimitLineBreaksCount(normalizedLineBreaks);
        var linksProcessed = ReplaceUrisWithBbCodeUrl(limitedLineBreaksCount);
        var crossLinksProcessed = threadId == null
            ? linksProcessed
            : ReplacePostLinksWithBbCodeUrl(categoryAlias, threadId.Value, linksProcessed);
        var convertedToHtml = _bbParser.Parse(crossLinksProcessed).ToHtml();
        return convertedToHtml;
    }

    public string MessageToPlainText(string text)
    {
        using var activity = WebTelemetry.MessagePostProcessorSource.StartActivity();
        var convertedBbToHtml = _bbParser.Parse(text).ToHtml();
        var extractedPlainText = HtmlUtilities.ConvertToPlainText(convertedBbToHtml);
        var normalizedLineBreaks = NormalizeLineBreaks(extractedPlainText);
        var limitedLineBreaksCount = LimitLineBreaksCount(normalizedLineBreaks);
        return limitedLineBreaksCount;
    }

    public IReadOnlyList<long> GetMentionedPosts(string text)
    {
        using var activity = WebTelemetry.MessagePostProcessorSource.StartActivity();
        var mentionedPosts = new List<long>();
        var matches = PostLinkRegex.Matches(text);
        foreach (Match match in matches)
        {
            if (long.TryParse(match.Groups[1].Value, out var postId))
            {
                mentionedPosts.Add(postId);
            }
        }

        return mentionedPosts;
    }
}
