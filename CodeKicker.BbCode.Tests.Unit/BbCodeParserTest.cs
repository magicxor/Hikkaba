using CodeKicker.BbCode.SyntaxTree;
using Xunit;

namespace CodeKicker.BbCode.Tests.Unit;

public class BbCodeParserTest
{
    [Fact]
    public void Test1()
    {
        Assert.Equal("", BbEncodeForTest("", ErrorMode.Strict));
    }

    [Fact]
    public void Test2()
    {
        Assert.Equal("a", BbEncodeForTest("a", ErrorMode.Strict));
        Assert.Equal(" a b c ", BbEncodeForTest(" a b c ", ErrorMode.Strict));
    }

    [Fact]
    public void Test3()
    {
        Assert.Equal("<b></b>", BbEncodeForTest("[b][/b]", ErrorMode.Strict));
    }

    [Fact]
    public void Test4()
    {
        Assert.Equal("text<b>text</b>text", BbEncodeForTest("text[b]text[/b]text", ErrorMode.Strict));
    }

    [Fact]
    public void Test5()
    {
        Assert.Equal("<a href=\"http://example.org/path?name=value\">text</a>", BbEncodeForTest("[url=http://example.org/path?name=value]text[/url]", ErrorMode.Strict));
    }

    [Fact]
    public void LeafElementWithoutContent()
    {
        Assert.Equal("xxxnameyyy", BbEncodeForTest("[placeholder=name]", ErrorMode.Strict));
        Assert.Equal("xxxyyy", BbEncodeForTest("[placeholder=]", ErrorMode.Strict));
        Assert.Equal("xxxyyy", BbEncodeForTest("[placeholder]", ErrorMode.Strict));
        Assert.Equal("axxxyyyb", BbEncodeForTest("a[placeholder]b", ErrorMode.Strict));
        Assert.Equal("<b>a</b>xxxyyy<b>b</b>", BbEncodeForTest("[b]a[/b][placeholder][b]b[/b]", ErrorMode.Strict));

        Assert.Throws<BbCodeParsingException>(() => BbEncodeForTest("[placeholder][/placeholder]", ErrorMode.Strict));
        Assert.Throws<BbCodeParsingException>(() => BbEncodeForTest("[placeholder/]", ErrorMode.Strict));
    }

    [Fact]
    public void ImgTagHasNoContent()
    {
        Assert.Equal("<img src=\"url\" />", BbEncodeForTest("[img]url[/img]", ErrorMode.Strict));
    }

    [Fact]
    public void ListItemIsAutoClosed()
    {
        Assert.Equal("<li>item</li>", BbEncodeForTest("[*]item", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, false));
        Assert.Equal("<ul><li>item</li></ul>", BbEncodeForTest("[list][*]item[/list]", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, false));
        Assert.Equal("<li>item</li>", BbEncodeForTest("[*]item[/*]", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, false));
        Assert.Equal("<li><li>item</li></li>", BbEncodeForTest("[*][*]item", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, false));
        Assert.Equal("<li>1<li>2</li></li>", BbEncodeForTest("[*]1[*]2", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, false));

        Assert.Equal("<li></li>item", BbEncodeForTest("[*]item", ErrorMode.Strict, BbTagClosingStyle.LeafElementWithoutContent, false));
        Assert.Equal("<ul><li></li>item</ul>", BbEncodeForTest("[list][*]item[/list]", ErrorMode.Strict, BbTagClosingStyle.LeafElementWithoutContent, false));
        Assert.Equal("<li></li><li></li>item", BbEncodeForTest("[*][*]item", ErrorMode.Strict, BbTagClosingStyle.LeafElementWithoutContent, false));
        Assert.Equal("<li></li>1<li></li>2", BbEncodeForTest("[*]1[*]2", ErrorMode.Strict, BbTagClosingStyle.LeafElementWithoutContent, false));

        Assert.Equal("<li>item</li>", BbEncodeForTest("[*]item", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, true));
        Assert.Equal("<ul><li>item</li></ul>", BbEncodeForTest("[list][*]item[/list]", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, true));
        Assert.Equal("<li>item</li>", BbEncodeForTest("[*]item[/*]", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, true));
        Assert.Equal("<li></li><li>item</li>", BbEncodeForTest("[*][*]item", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, true));
        Assert.Equal("<li>1</li><li>2</li>", BbEncodeForTest("[*]1[*]2", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, true));
        Assert.Equal("<li>1<b>a</b></li><li>2</li>", BbEncodeForTest("[*]1[b]a[/b][*]2", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, true));
        Assert.Equal("<li>1<b>a</b></li><li>2</li>", BbEncodeForTest("[*]1[b]a[*]2", ErrorMode.ErrorFree, BbTagClosingStyle.AutoCloseElement, true));

        Assert.Throws<BbCodeParsingException>(() => BbEncodeForTest("[*]1[b]a[*]2", ErrorMode.Strict, BbTagClosingStyle.AutoCloseElement, true));
    }

    [Fact]
    public void TagContentTransformer()
    {
        var parser = new BbCodeParser([
            new BbTag("b", "<b>", "</b>", true, true, content => content.Trim()),
        ]);

        Assert.Equal("<b>abc</b>", parser.ToHtml("[b] abc [/b]"));
    }

    [Fact]
    public void AttributeValueTransformer()
    {
        var parser = new BbCodeParser(ErrorMode.Strict, null, [
            new BbTag("font", "<span style=\"${color}${font}\">", "</span>", true, true,
                new BbAttribute("color", "color", attributeRenderingContext => string.IsNullOrEmpty(attributeRenderingContext.AttributeValue) ? "" : "color:" + attributeRenderingContext.AttributeValue + ";"),
                new BbAttribute("font", "font", attributeRenderingContext => string.IsNullOrEmpty(attributeRenderingContext.AttributeValue) ? "" : "font-family:" + attributeRenderingContext.AttributeValue + ";")),
        ]);

        Assert.Equal("<span style=\"color:red;font-family:Arial;\">abc</span>", parser.ToHtml("[font color=red font=Arial]abc[/font]"));
        Assert.Equal("<span style=\"color:red;\">abc</span>", parser.ToHtml("[font color=red]abc[/font]"));
    }

    private static void AssertTextNodesNotSplit(SyntaxTreeNode node)
    {
        if (node.SubNodes != null)
        {
            SyntaxTreeNode lastNode = null;
            for (int i = 0; i < node.SubNodes.Count; i++)
            {
                AssertTextNodesNotSplit(node.SubNodes[i]);
                if (lastNode != null)
                {
                    Assert.False(lastNode is TextNode && node.SubNodes[i] is TextNode);
                }

                lastNode = node.SubNodes[i];
            }
        }
    }

    public static string BbEncodeForTest(string bbCode, ErrorMode errorMode)
    {
        return BbEncodeForTest(bbCode, errorMode, BbTagClosingStyle.AutoCloseElement, false);
    }
    public static string BbEncodeForTest(string bbCode, ErrorMode errorMode, BbTagClosingStyle listItemBbTagClosingStyle, bool enableIterationElementBehavior)
    {
        return BbCodeTestUtil.GetParserForTest(errorMode, true, listItemBbTagClosingStyle, enableIterationElementBehavior).ToHtml(bbCode).Replace("\r", "").Replace("\n", "<br/>");
    }

    [Fact]
    public void StrictErrorMode()
    {
        Assert.True(BbCodeTestUtil.IsValid(@"", ErrorMode.Strict));
        Assert.True(BbCodeTestUtil.IsValid(@"[b]abc[/b]", ErrorMode.Strict));
        Assert.False(BbCodeTestUtil.IsValid(@"[b]abc", ErrorMode.Strict));
        Assert.False(BbCodeTestUtil.IsValid(@"abc[0]def", ErrorMode.Strict));
        Assert.False(BbCodeTestUtil.IsValid(@"\", ErrorMode.Strict));
        Assert.False(BbCodeTestUtil.IsValid(@"\x", ErrorMode.Strict));
        Assert.False(BbCodeTestUtil.IsValid(@"[", ErrorMode.Strict));
        Assert.False(BbCodeTestUtil.IsValid(@"]", ErrorMode.Strict));
    }

    [Fact]
    public void CorrectingErrorMode()
    {
        Assert.True(BbCodeTestUtil.IsValid(@"", ErrorMode.TryErrorCorrection));
        Assert.True(BbCodeTestUtil.IsValid(@"[b]abc[/b]", ErrorMode.TryErrorCorrection));
        Assert.True(BbCodeTestUtil.IsValid(@"[b]abc", ErrorMode.TryErrorCorrection));

        Assert.Equal(@"\", BbEncodeForTest(@"\", ErrorMode.TryErrorCorrection));
        Assert.Equal(@"\x", BbEncodeForTest(@"\x", ErrorMode.TryErrorCorrection));
        Assert.Equal(@"\", BbEncodeForTest(@"\\", ErrorMode.TryErrorCorrection));
    }

    [Fact]
    public void CorrectingErrorMode_EscapeCharsIgnored()
    {
        Assert.Equal(@"\\", BbEncodeForTest(@"\\\\", ErrorMode.TryErrorCorrection));
        Assert.Equal(@"\", BbEncodeForTest(@"\", ErrorMode.TryErrorCorrection));
        Assert.Equal(@"\x", BbEncodeForTest(@"\x", ErrorMode.TryErrorCorrection));
        Assert.Equal(@"\", BbEncodeForTest(@"\\", ErrorMode.TryErrorCorrection));
        Assert.Equal(@"[", BbEncodeForTest(@"\[", ErrorMode.TryErrorCorrection));
        Assert.Equal(@"]", BbEncodeForTest(@"\]", ErrorMode.TryErrorCorrection));
    }

    [Fact]
    public void TextNodeHtmlTemplate()
    {
        var parserNull = new BbCodeParser(ErrorMode.Strict, null, [
            new BbTag("b", "<b>", "</b>"),
        ]);
        var parserEmpty = new BbCodeParser(ErrorMode.Strict, "", [
            new BbTag("b", "<b>", "</b>"),
        ]);
        var parserDiv = new BbCodeParser(ErrorMode.Strict, "<div>${content}</div>", [
            new BbTag("b", "<b>", "</b>"),
        ]);

        Assert.Equal(@"", parserNull.ToHtml(@""));
        Assert.Equal(@"abc", parserNull.ToHtml(@"abc"));
        Assert.Equal(@"abc<b>def</b>", parserNull.ToHtml(@"abc[b]def[/b]"));

        Assert.Equal(@"", parserEmpty.ToHtml(@""));
        Assert.Equal(@"", parserEmpty.ToHtml(@"abc"));
        Assert.Equal(@"<b></b>", parserEmpty.ToHtml(@"abc[b]def[/b]"));

        Assert.Equal(@"", parserDiv.ToHtml(@""));
        Assert.Equal(@"<div>abc</div>", parserDiv.ToHtml(@"abc"));
        Assert.Equal(@"<div>abc</div><b><div>def</div></b>", parserDiv.ToHtml(@"abc[b]def[/b]"));
    }

    [Fact]
    public void ContentTransformer_EmptyAttribute_CanChooseValueFromAttributeRenderingContext()
    {
        var parser = BbCodeTestUtil.GetParserForTest(ErrorMode.Strict, true, BbTagClosingStyle.AutoCloseElement, false);

        Assert.Equal(@"<a href=""http://codekicker.de"">http://codekicker.de</a>", parser.ToHtml(@"[url2]http://codekicker.de[/url2]"));
        Assert.Equal(@"<a href=""http://codekicker.de"">http://codekicker.de</a>", parser.ToHtml(@"[url2=http://codekicker.de]http://codekicker.de[/url2]"));
    }

    [Fact]
    public void StopProcessingDirective_StopsParserProcessingTagLikeText_UntilClosingTag()
    {
        var parser = new BbCodeParser(ErrorMode.ErrorFree, null, [new BbTag("code", "<pre>", "</pre>") { StopProcessing = true }]);
        var input = "[code][i]This should [u]be a[/u] text literal[/i][/code]";
        var expected = "<pre>[i]This should [u]be a[/u] text literal[/i]</pre>";
        Assert.Equal(expected, parser.ToHtml(input));
    }
}
