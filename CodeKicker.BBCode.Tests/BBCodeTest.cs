using System;
using System.Collections.Generic;
using CodeKicker.BBCode.SyntaxTree;
using Xunit;

namespace CodeKicker.BBCode.Tests;

public class BbCodeTest
{
    [Fact]
    public void ReplaceTextSpans_ManualTestCases()
    {
        ReplaceTextSpans_ManualTestCases_TestCase("", "", null, null);
        ReplaceTextSpans_ManualTestCases_TestCase("a", "a", null, null);
        ReplaceTextSpans_ManualTestCases_TestCase("[b]a[/b]", "[b]a[/b]", null, null);
        ReplaceTextSpans_ManualTestCases_TestCase("[b]a[/b]", "[b]a[/b]", _ => [new TextSpanReplaceInfo(0, 0, null)], null);
        ReplaceTextSpans_ManualTestCases_TestCase("[b]a[/b]", "[b][/b]", _ => [new TextSpanReplaceInfo(0, 1, null)], null);
        ReplaceTextSpans_ManualTestCases_TestCase("[b]a[/b]", "[b]x[/b]", _ => [new TextSpanReplaceInfo(0, 1, new TextNode("x"))], null);

        ReplaceTextSpans_ManualTestCases_TestCase("abc[b]def[/b]ghi[i]jkl[/i]", "xyabc[b]z[/b]g2i1[i]jkl[/i]",
            txt =>
                txt switch
                {
                    "abc" => [new TextSpanReplaceInfo(0, 0, new TextNode("x")), new TextSpanReplaceInfo(0, 0, new TextNode("y"))],
                    "def" => [new TextSpanReplaceInfo(0, 3, new TextNode("z"))],
                    "ghi" => [new TextSpanReplaceInfo(1, 1, new TextNode("2")), new TextSpanReplaceInfo(3, 0, new TextNode("1"))],
                    "jkl" => new[] { new TextSpanReplaceInfo(0, 0, new TextNode("w")) },
                    _ => null,
                },
            tagNode => tagNode.Tag.Name != "i");
    }

    private static void ReplaceTextSpans_ManualTestCases_TestCase(string bbCode, string expected, Func<string, IList<TextSpanReplaceInfo>> getTextSpansToReplace, Func<TagNode, bool> tagFilter)
    {
        var tree1 = BbCodeTestUtil.GetParserForTest(ErrorMode.Strict, false, BbTagClosingStyle.AutoCloseElement, false).ParseSyntaxTree(bbCode);
        var tree2 = CodeKicker.BBCode.BbCode.ReplaceTextSpans(tree1, getTextSpansToReplace ?? (_ => Array.Empty<TextSpanReplaceInfo>()), tagFilter);
        Assert.Equal(expected, tree2.ToBbCode());
    }

    [Fact]
    public void ReplaceTextSpans_WhenNoModifications_TreeIsPreserved()
    {
        var tree1 = BbCodeTestUtil.GetAnyTree();
        var tree2 = CodeKicker.BBCode.BbCode.ReplaceTextSpans(tree1, _ => Array.Empty<TextSpanReplaceInfo>(), null);
        Assert.Same(tree1, tree2);
    }

    [Fact]
    public void ReplaceTextSpans_WhenEmptyModifications_TreeIsPreserved()
    {
        var tree1 = BbCodeTestUtil.GetAnyTree();
        var tree2 = CodeKicker.BBCode.BbCode.ReplaceTextSpans(tree1, _ => [new TextSpanReplaceInfo(0, 0, null)], null);
        Assert.Equal(tree1.ToBbCode(), tree2.ToBbCode());
    }

    [Fact]
    public void ReplaceTextSpans_WhenEverythingIsConvertedToX_OutputContainsOnlyX_CheckedWithContains()
    {
        var tree1 = BbCodeTestUtil.GetAnyTree();
        var tree2 = CodeKicker.BBCode.BbCode.ReplaceTextSpans(tree1, txt => [new TextSpanReplaceInfo(0, txt.Length, new TextNode("x"))], null);
        Assert.True(!tree2.ToBbCode().Contains('a'));
    }

    [Fact]
    public void ReplaceTextSpans_WhenEverythingIsConvertedToX_OutputContainsOnlyX_CheckedWithTreeWalk()
    {
        var tree1 = BbCodeTestUtil.GetAnyTree();
        var tree2 = CodeKicker.BBCode.BbCode.ReplaceTextSpans(tree1, txt => [new TextSpanReplaceInfo(0, txt.Length, new TextNode("x"))], null);
        new TextAssertVisitor(str => Assert.Equal("x", str)).Visit(tree2);
    }

    private class TextAssertVisitor : SyntaxTreeVisitor
    {
        private readonly Action<string> _assertFunction;

        public TextAssertVisitor(Action<string> assertFunction)
        {
            this._assertFunction = assertFunction;
        }

        protected override SyntaxTreeNode Visit(TextNode node)
        {
            _assertFunction(node.Text);
            return node;
        }
    }
}
