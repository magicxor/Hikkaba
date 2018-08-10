using System.Linq;
using CodeKicker.BBCode.SyntaxTree;
using Xunit;

namespace CodeKicker.BBCode.Tests
{
    public partial class SyntaxTreeVisitorTest
    {
        [Fact]
        public void DefaultVisitorModifiesNothing()
        {
            var tree = BBCodeTestUtil.GetAnyTree();
            var tree2 = new SyntaxTreeVisitor().Visit(tree);
            Assert.True(ReferenceEquals(tree, tree2));
        }

        [Fact]
        public void TextModifiedTreesAreNotEqual()
        {
            var tree = BBCodeTestUtil.GetAnyTree();
            var tree2 = new TextModificationSyntaxTreeVisitor().Visit(tree);
            Assert.True(tree != tree2);
        }

        class TextModificationSyntaxTreeVisitor : SyntaxTreeVisitor
        {
            protected override SyntaxTreeNode Visit(TextNode node)
            {
                return new TextNode(node.Text + "x", node.HtmlTemplate);
            }
            protected override SyntaxTreeNode Visit(SequenceNode node)
            {
                var baseResult = base.Visit(node);
                return baseResult.SetSubNodes(baseResult.SubNodes.Concat(new[] { new TextNode("y") }));
            }
            protected override SyntaxTreeNode Visit(TagNode node)
            {
                var baseResult = base.Visit(node);
                return baseResult.SetSubNodes(baseResult.SubNodes.Concat(new[] { new TextNode("z") }));
            }
        }
    }
}
