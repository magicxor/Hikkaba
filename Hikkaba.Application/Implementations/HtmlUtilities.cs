using Hikkaba.Application.Telemetry;
using HtmlAgilityPack;

namespace Hikkaba.Application.Implementations;

public static class HtmlUtilities
{
    private const string LineBreak = "\r\n";

    /// <summary>
    /// Converts HTML to plain text / strips tags.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns></returns>
    public static string ConvertToPlainText(string html)
    {
        using var activity = ApplicationTelemetry.HtmlUtilitiesSource.StartActivity();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        using var sw = new StringWriter();
        ConvertTo(doc.DocumentNode, sw);
        sw.Flush();

        return sw.ToString();
    }

    private static void ConvertContentTo(HtmlNode node, TextWriter outText)
    {
        foreach (var childNode in node.ChildNodes)
        {
            ConvertTo(childNode, outText);
        }
    }

    private static void ConvertTo(HtmlNode node, TextWriter outText)
    {
        switch (node.NodeType)
        {
            case HtmlNodeType.Comment:
                // don't output comments
                break;

            case HtmlNodeType.Document:
                ConvertContentTo(node, outText);
                break;

            case HtmlNodeType.Text:
                // script and style must not be output
                var parentName = node.ParentNode.Name;

                if (parentName is "script" or "style")
                    break;

                // get text
                var html = ((HtmlTextNode)node).Text;

                // is it in fact a special closing node output as text?
                if (HtmlNode.IsOverlappedClosingElement(html))
                    break;

                // check the text is meaningful and not a bunch of whitespaces
                if (html.Trim().Length > 0)
                {
                    outText.Write(HtmlEntity.DeEntitize(html));
                }

                break;

            case HtmlNodeType.Element:
                switch (node.Name)
                {
                    case "p":
                    case "br":
                        outText.Write(LineBreak);
                        break;
                }

                if (node.HasChildNodes)
                {
                    ConvertContentTo(node, outText);
                }

                break;
        }
    }
}
