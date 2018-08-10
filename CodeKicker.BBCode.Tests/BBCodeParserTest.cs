using CodeKicker.BBCode.SyntaxTree;
using Xunit;

namespace CodeKicker.BBCode.Tests
{
    public partial class BBCodeParserTest
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal("", BBEncodeForTest("", ErrorMode.Strict));
        }

        [Fact]
        public void Test2()
        {
            Assert.Equal("a", BBEncodeForTest("a", ErrorMode.Strict));
            Assert.Equal(" a b c ", BBEncodeForTest(" a b c ", ErrorMode.Strict));
        }

        [Fact]
        public void Test3()
        {
            Assert.Equal("<b></b>", BBEncodeForTest("[b][/b]", ErrorMode.Strict));
        }

        [Fact]
        public void Test4()
        {
            Assert.Equal("text<b>text</b>text", BBEncodeForTest("text[b]text[/b]text", ErrorMode.Strict));
        }

        [Fact]
        public void Test5()
        {
            Assert.Equal("<a href=\"http://example.org/path?name=value\">text</a>", BBEncodeForTest("[url=http://example.org/path?name=value]text[/url]", ErrorMode.Strict));
        }

        [Fact]
        public void LeafElementWithoutContent()
        {
            Assert.Equal("xxxnameyyy", BBEncodeForTest("[placeholder=name]", ErrorMode.Strict));
            Assert.Equal("xxxyyy", BBEncodeForTest("[placeholder=]", ErrorMode.Strict));
            Assert.Equal("xxxyyy", BBEncodeForTest("[placeholder]", ErrorMode.Strict));
            Assert.Equal("axxxyyyb", BBEncodeForTest("a[placeholder]b", ErrorMode.Strict));
            Assert.Equal("<b>a</b>xxxyyy<b>b</b>", BBEncodeForTest("[b]a[/b][placeholder][b]b[/b]", ErrorMode.Strict));

            Assert.Throws<BBCodeParsingException>(() => BBEncodeForTest("[placeholder][/placeholder]", ErrorMode.Strict));
            Assert.Throws<BBCodeParsingException>(() => BBEncodeForTest("[placeholder/]", ErrorMode.Strict));
        }

        [Fact]
        public void ImgTagHasNoContent()
        {
            Assert.Equal("<img src=\"url\" />", BBEncodeForTest("[img]url[/img]", ErrorMode.Strict));
        }

        [Fact]
        public void ListItemIsAutoClosed()
        {
            Assert.Equal("<li>item</li>", BBEncodeForTest("[*]item", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, false));
            Assert.Equal("<ul><li>item</li></ul>", BBEncodeForTest("[list][*]item[/list]", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, false));
            Assert.Equal("<li>item</li>", BBEncodeForTest("[*]item[/*]", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, false));
            Assert.Equal("<li><li>item</li></li>", BBEncodeForTest("[*][*]item", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, false));
            Assert.Equal("<li>1<li>2</li></li>", BBEncodeForTest("[*]1[*]2", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, false));

            Assert.Equal("<li></li>item", BBEncodeForTest("[*]item", ErrorMode.Strict, BBTagClosingStyle.LeafElementWithoutContent, false));
            Assert.Equal("<ul><li></li>item</ul>", BBEncodeForTest("[list][*]item[/list]", ErrorMode.Strict, BBTagClosingStyle.LeafElementWithoutContent, false));
            Assert.Equal("<li></li><li></li>item", BBEncodeForTest("[*][*]item", ErrorMode.Strict, BBTagClosingStyle.LeafElementWithoutContent, false));
            Assert.Equal("<li></li>1<li></li>2", BBEncodeForTest("[*]1[*]2", ErrorMode.Strict, BBTagClosingStyle.LeafElementWithoutContent, false));

            Assert.Equal("<li>item</li>", BBEncodeForTest("[*]item", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, true));
            Assert.Equal("<ul><li>item</li></ul>", BBEncodeForTest("[list][*]item[/list]", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, true));
            Assert.Equal("<li>item</li>", BBEncodeForTest("[*]item[/*]", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, true));
            Assert.Equal("<li></li><li>item</li>", BBEncodeForTest("[*][*]item", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, true));
            Assert.Equal("<li>1</li><li>2</li>", BBEncodeForTest("[*]1[*]2", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, true));
            Assert.Equal("<li>1<b>a</b></li><li>2</li>", BBEncodeForTest("[*]1[b]a[/b][*]2", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, true));
            Assert.Equal("<li>1<b>a</b></li><li>2</li>", BBEncodeForTest("[*]1[b]a[*]2", ErrorMode.ErrorFree, BBTagClosingStyle.AutoCloseElement, true));

            Assert.Throws<BBCodeParsingException>(() => BBEncodeForTest("[*]1[b]a[*]2", ErrorMode.Strict, BBTagClosingStyle.AutoCloseElement, true));
        }

        [Fact]
        public void TagContentTransformer()
        {
            var parser = new BBCodeParser(new[]
                {
                    new BBTag("b", "<b>", "</b>", true, true, content => content.Trim()), 
                });

            Assert.Equal("<b>abc</b>", parser.ToHtml("[b] abc [/b]"));
        }

        [Fact]
        public void AttributeValueTransformer()
        {
            var parser = new BBCodeParser(ErrorMode.Strict, null, new[]
                {
                    new BBTag("font", "<span style=\"${color}${font}\">", "</span>", true, true,
                        new BBAttribute("color", "color", attributeRenderingContext => string.IsNullOrEmpty(attributeRenderingContext.AttributeValue) ? "" : "color:" + attributeRenderingContext.AttributeValue + ";"),
                        new BBAttribute("font", "font", attributeRenderingContext => string.IsNullOrEmpty(attributeRenderingContext.AttributeValue) ? "" : "font-family:" + attributeRenderingContext.AttributeValue + ";")),
                });

            Assert.Equal("<span style=\"color:red;font-family:Arial;\">abc</span>", parser.ToHtml("[font color=red font=Arial]abc[/font]"));
            Assert.Equal("<span style=\"color:red;\">abc</span>", parser.ToHtml("[font color=red]abc[/font]"));
        }

        static void AssertTextNodesNotSplit(SyntaxTreeNode node)
        {
            if (node.SubNodes != null)
            {
                SyntaxTreeNode lastNode = null;
                for (int i = 0; i < node.SubNodes.Count; i++)
                {
                    AssertTextNodesNotSplit(node.SubNodes[i]);
                    if (lastNode != null)
                        Assert.False(lastNode is TextNode && node.SubNodes[i] is TextNode);
                    lastNode = node.SubNodes[i];
                }
            }
        }

        public static string BBEncodeForTest(string bbCode, ErrorMode errorMode)
        {
            return BBEncodeForTest(bbCode, errorMode, BBTagClosingStyle.AutoCloseElement, false);
        }
        public static string BBEncodeForTest(string bbCode, ErrorMode errorMode, BBTagClosingStyle listItemBbTagClosingStyle, bool enableIterationElementBehavior)
        {
            return BBCodeTestUtil.GetParserForTest(errorMode, true, listItemBbTagClosingStyle, enableIterationElementBehavior).ToHtml(bbCode).Replace("\r", "").Replace("\n", "<br/>");
        }

        [Fact]
        public void StrictErrorMode()
        {
            Assert.True(BBCodeTestUtil.IsValid(@"", ErrorMode.Strict));
            Assert.True(BBCodeTestUtil.IsValid(@"[b]abc[/b]", ErrorMode.Strict));
            Assert.False(BBCodeTestUtil.IsValid(@"[b]abc", ErrorMode.Strict));
            Assert.False(BBCodeTestUtil.IsValid(@"abc[0]def", ErrorMode.Strict));
            Assert.False(BBCodeTestUtil.IsValid(@"\", ErrorMode.Strict));
            Assert.False(BBCodeTestUtil.IsValid(@"\x", ErrorMode.Strict));
            Assert.False(BBCodeTestUtil.IsValid(@"[", ErrorMode.Strict));
            Assert.False(BBCodeTestUtil.IsValid(@"]", ErrorMode.Strict));
        }

        [Fact]
        public void CorrectingErrorMode()
        {
            Assert.True(BBCodeTestUtil.IsValid(@"", ErrorMode.TryErrorCorrection));
            Assert.True(BBCodeTestUtil.IsValid(@"[b]abc[/b]", ErrorMode.TryErrorCorrection));
            Assert.True(BBCodeTestUtil.IsValid(@"[b]abc", ErrorMode.TryErrorCorrection));

            Assert.Equal(@"\", BBEncodeForTest(@"\", ErrorMode.TryErrorCorrection));
            Assert.Equal(@"\x", BBEncodeForTest(@"\x", ErrorMode.TryErrorCorrection));
            Assert.Equal(@"\", BBEncodeForTest(@"\\", ErrorMode.TryErrorCorrection));
        }

        [Fact]
        public void CorrectingErrorMode_EscapeCharsIgnored()
        {
            Assert.Equal(@"\\", BBEncodeForTest(@"\\\\", ErrorMode.TryErrorCorrection));
            Assert.Equal(@"\", BBEncodeForTest(@"\", ErrorMode.TryErrorCorrection));
            Assert.Equal(@"\x", BBEncodeForTest(@"\x", ErrorMode.TryErrorCorrection));
            Assert.Equal(@"\", BBEncodeForTest(@"\\", ErrorMode.TryErrorCorrection));
            Assert.Equal(@"[", BBEncodeForTest(@"\[", ErrorMode.TryErrorCorrection));
            Assert.Equal(@"]", BBEncodeForTest(@"\]", ErrorMode.TryErrorCorrection));
        }

        [Fact]
        public void TextNodeHtmlTemplate()
        {
            var parserNull = new BBCodeParser(ErrorMode.Strict, null, new[]
                {
                    new BBTag("b", "<b>", "</b>"), 
                });
            var parserEmpty = new BBCodeParser(ErrorMode.Strict, "", new[]
                {
                    new BBTag("b", "<b>", "</b>"), 
                });
            var parserDiv = new BBCodeParser(ErrorMode.Strict, "<div>${content}</div>", new[]
                {
                    new BBTag("b", "<b>", "</b>"), 
                });

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
            var parser = BBCodeTestUtil.GetParserForTest(ErrorMode.Strict, true, BBTagClosingStyle.AutoCloseElement, false);

            Assert.Equal(@"<a href=""http://codekicker.de"">http://codekicker.de</a>", parser.ToHtml(@"[url2]http://codekicker.de[/url2]"));
            Assert.Equal(@"<a href=""http://codekicker.de"">http://codekicker.de</a>", parser.ToHtml(@"[url2=http://codekicker.de]http://codekicker.de[/url2]"));
        }

        [Fact]
        public void StopProcessingDirective_StopsParserProcessingTagLikeText_UntilClosingTag()
        {
            var parser = new BBCodeParser(ErrorMode.ErrorFree, null, new[] { new BBTag("code", "<pre>", "</pre>") { StopProcessing = true } });
            var input = "[code][i]This should [u]be a[/u] text literal[/i][/code]";
            var expected = "<pre>[i]This should [u]be a[/u] text literal[/i]</pre>";
            Assert.Equal(expected, parser.ToHtml(input));
        }
    }
}
