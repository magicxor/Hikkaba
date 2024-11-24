using System;
using System.Collections.Generic;
using CodeKicker.BBCode.SyntaxTree;
using Xunit;

namespace CodeKicker.BBCode.Tests;

public partial class BBCodeTest
{
    private static BBCodeParser GetSimpleParser()
    {
        return new BBCodeParser(new List<BBTag>());
    }

    [Fact]
    public void ReplaceTextSpans_ManualTestCases()
    {
        ReplaceTextSpans_ManualTestCases_TestCase("", "", null, null);
        ReplaceTextSpans_ManualTestCases_TestCase("a", "a", null, null);
        ReplaceTextSpans_ManualTestCases_TestCase("[b]a[/b]", "[b]a[/b]", null, null);
        ReplaceTextSpans_ManualTestCases_TestCase("[b]a[/b]", "[b]a[/b]", _ => new[] { new TextSpanReplaceInfo(0, 0, null) }, null);
        ReplaceTextSpans_ManualTestCases_TestCase("[b]a[/b]", "[b][/b]", _ => new[] { new TextSpanReplaceInfo(0, 1, null) }, null);
        ReplaceTextSpans_ManualTestCases_TestCase("[b]a[/b]", "[b]x[/b]", _ => new[] { new TextSpanReplaceInfo(0, 1, new TextNode("x")) }, null);

        ReplaceTextSpans_ManualTestCases_TestCase("abc[b]def[/b]ghi[i]jkl[/i]", "xyabc[b]z[/b]g2i1[i]jkl[/i]",
            txt =>
                txt == "abc" ? new[] { new TextSpanReplaceInfo(0, 0, new TextNode("x")), new TextSpanReplaceInfo(0, 0, new TextNode("y")) } :
                txt == "def" ? new[] { new TextSpanReplaceInfo(0, 3, new TextNode("z")) } :
                txt == "ghi" ? new[] { new TextSpanReplaceInfo(1, 1, new TextNode("2")), new TextSpanReplaceInfo(3, 0, new TextNode("1")) } :
                txt == "jkl" ? new[] { new TextSpanReplaceInfo(0, 0, new TextNode("w")) } :
                null,
            tagNode => tagNode.Tag.Name != "i");
    }

    private static void ReplaceTextSpans_ManualTestCases_TestCase(string bbCode, string expected, Func<string, IList<TextSpanReplaceInfo>> getTextSpansToReplace, Func<TagNode, bool> tagFilter)
    {
        var tree1 = BBCodeTestUtil.GetParserForTest(ErrorMode.Strict, false, BBTagClosingStyle.AutoCloseElement, false).ParseSyntaxTree(bbCode);
        var tree2 = CodeKicker.BBCode.BBCode.ReplaceTextSpans(tree1, getTextSpansToReplace ?? (_ => new TextSpanReplaceInfo[0]), tagFilter);
        Assert.Equal(expected, tree2.ToBBCode());
    }

    [Fact]
    public void ReplaceTextSpans_WhenNoModifications_TreeIsPreserved()
    {
        var tree1 = BBCodeTestUtil.GetAnyTree();
        var tree2 = CodeKicker.BBCode.BBCode.ReplaceTextSpans(tree1, _ => new TextSpanReplaceInfo[0], null);
        Assert.Same(tree1, tree2);
    }

    [Fact]
    public void ReplaceTextSpans_WhenEmptyModifications_TreeIsPreserved()
    {
        var tree1 = BBCodeTestUtil.GetAnyTree();
        var tree2 = CodeKicker.BBCode.BBCode.ReplaceTextSpans(tree1, _ => new[] { new TextSpanReplaceInfo(0, 0, null) }, null);
        Assert.Equal(tree1.ToBBCode(), tree2.ToBBCode());
    }

    [Fact]
    public void ReplaceTextSpans_WhenEverythingIsConvertedToX_OutputContainsOnlyX_CheckedWithContains()
    {
        var tree1 = BBCodeTestUtil.GetAnyTree();
        var tree2 = CodeKicker.BBCode.BBCode.ReplaceTextSpans(tree1, txt => new[] { new TextSpanReplaceInfo(0, txt.Length, new TextNode("x")) }, null);
        Assert.True(!tree2.ToBBCode().Contains("a"));
    }

    [Fact]
    public void ReplaceTextSpans_WhenEverythingIsConvertedToX_OutputContainsOnlyX_CheckedWithTreeWalk()
    {
        var tree1 = BBCodeTestUtil.GetAnyTree();
        var tree2 = CodeKicker.BBCode.BBCode.ReplaceTextSpans(tree1, txt => new[] { new TextSpanReplaceInfo(0, txt.Length, new TextNode("x")) }, null);
        new TextAssertVisitor(str => Assert.True(str == "x")).Visit(tree2);
    }

    private class TextAssertVisitor : SyntaxTreeVisitor
    {
        private Action<string> assertFunction;

        public TextAssertVisitor(Action<string> assertFunction)
        {
            this.assertFunction = assertFunction;
        }

        protected override SyntaxTreeNode Visit(TextNode node)
        {
            assertFunction(node.Text);
            return node;
        }
    }
}