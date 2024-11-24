using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeKicker.BBCode.SyntaxTree;

public sealed class TextNode : SyntaxTreeNode
{
    public TextNode(string text)
        : this(text, null)
    {
    }
    public TextNode(string text, string htmlTemplate)
        : base(null)
    {
        ArgumentNullException.ThrowIfNull(text);
        Text = text;
        HtmlTemplate = htmlTemplate;
    }

    public string Text { get; private set; }
    public string HtmlTemplate { get; private set; }

    public override string ToHtml()
    {
        return HtmlTemplate == null ? HttpUtility.HtmlEncode(Text) : HtmlTemplate.Replace("${content}", HttpUtility.HtmlEncode(Text));
    }
    public override string ToBbCode()
    {
        return Text.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]");
    }
    public override string ToText()
    {
        return Text;
    }

    public override SyntaxTreeNode SetSubNodes(IEnumerable<SyntaxTreeNode> subNodes)
    {
        ArgumentNullException.ThrowIfNull(subNodes);
        if (subNodes.Any()) throw new ArgumentException("subNodes cannot contain any nodes for a TextNode");
        return this;
    }
    internal override SyntaxTreeNode AcceptVisitor(SyntaxTreeVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.Visit(this);
    }

    protected override bool EqualsCore(SyntaxTreeNode b)
    {
        var casted = (TextNode)b;
        return Text == casted.Text && HtmlTemplate == casted.HtmlTemplate;
    }
}
