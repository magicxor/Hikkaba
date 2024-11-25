using System;
using System.Collections.Generic;
using System.Linq;
using CodeKicker.BbCode.SyntaxTree;

namespace CodeKicker.BbCode.Tests.Unit;

public static class BbCodeTestUtil
{
    public static SequenceNode CreateRootNode(BbTag[] allowedTags)
    {
        var node = new SequenceNode();
        AddSubnodes(allowedTags, node);
        return node;
    }

    private static SyntaxTreeNode CreateNode(BbTag[] allowedTags, bool allowText)
    {
        var tag = allowedTags.First();
        var node = new TagNode(tag);

        if (tag.Attributes != null)
        {
            var selectedIds = new List<string>();
            foreach (var attr in tag.Attributes)
            {
                if (!selectedIds.Contains(attr.Id))
                {
                    var val = "hello";
                    node.AttributeValues[attr] = val;
                    selectedIds.Add(attr.Id);
                }
            }
        }
        return node;
    }

    private static void AddSubnodes(BbTag[] allowedTags, SyntaxTreeNode node)
    {
        int count = 3;
        bool lastWasText = false;
        for (int i = 0; i < count; i++)
        {
            var subNode = CreateNode(allowedTags, !lastWasText);
            lastWasText = subNode is TextNode;
            node.SubNodes.Add(subNode);
        }
    }

    public static BbCodeParser GetParserForTest(ErrorMode errorMode, bool includePlaceholder, BbTagClosingStyle listItemBbTagClosingStyle, bool enableIterationElementBehavior)
    {
        return new BbCodeParser(errorMode, null, new[]
        {
            new BbTag("b", "<b>", "</b>"),
            new BbTag("i", "<span style=\"font-style:italic;\">", "</span>"),
            new BbTag("u", "<span style=\"text-decoration:underline;\">", "</span>"),
            new BbTag("code", "<pre class=\"prettyprint\">", "</pre>"),
            new BbTag("img", "<img src=\"${content}\" />", "", false, true),
            new BbTag("quote", "<blockquote>", "</blockquote>"),
            new BbTag("list", "<ul>", "</ul>"),
            new BbTag("*", "<li>", "</li>", true, listItemBbTagClosingStyle, null, enableIterationElementBehavior),
            new BbTag("url", "<a href=\"${href}\">", "</a>", new BbAttribute("href", ""), new BbAttribute("href", "href")),
            new BbTag("url2", "<a href=\"${href}\">", "</a>", new BbAttribute("href", "", GetUrl2Href), new BbAttribute("href", "href", GetUrl2Href)),
            !includePlaceholder ? null : new BbTag("placeholder", "${name}", "", false, BbTagClosingStyle.LeafElementWithoutContent, null, new BbAttribute("name", "", name => "xxx" + name.AttributeValue + "yyy")),
        }.Where(x => x != null).ToArray());
    }

    private static string GetUrl2Href(IAttributeRenderingContext attributeRenderingContext)
    {
        if (!string.IsNullOrEmpty(attributeRenderingContext.AttributeValue))
        {
            return attributeRenderingContext.AttributeValue;
        }

        var content = attributeRenderingContext.GetAttributeValueById(BbTag.ContentPlaceholderName);
        if (!string.IsNullOrEmpty(content) && content.StartsWith("http:", StringComparison.Ordinal))
        {
            return content;
        }

        return null;
    }

    public static BbCodeParser GetSimpleParserForTest(ErrorMode errorMode)
    {
        return new BbCodeParser(errorMode, null, [
            new BbTag("x", "${content}${x}", "${y}", true, true, new BbAttribute("x", "x"), new BbAttribute("y", "y", x => x.AttributeValue)),
        ]);
    }

    public static string SimpleBbEncodeForTest(string bbCode, ErrorMode errorMode)
    {
        return GetSimpleParserForTest(errorMode).ToHtml(bbCode);
    }

    public static bool IsValid(string bbCode, ErrorMode errorMode)
    {
        try
        {
            BbCodeParserTest.BbEncodeForTest(bbCode, errorMode);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static SequenceNode GetAnyTree()
    {
        var parser = GetParserForTest(ErrorMode.Strict, true, BbTagClosingStyle.AutoCloseElement, false);
        return CreateRootNode(parser.Tags.ToArray());
    }
}
