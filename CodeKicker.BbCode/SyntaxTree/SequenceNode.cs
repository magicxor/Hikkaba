using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeKicker.BbCode.SyntaxTree;

public sealed class SequenceNode : SyntaxTreeNode
{
    public SequenceNode()
    {
    }

    public SequenceNode(SyntaxTreeNodeCollection subNodes)
        : base(subNodes)
    {
        ArgumentNullException.ThrowIfNull(subNodes);
    }

    public SequenceNode(IEnumerable<SyntaxTreeNode> subNodes)
        : base(subNodes)
    {
        ArgumentNullException.ThrowIfNull(subNodes);
    }

    public override string ToHtml()
    {
        return string.Concat(SubNodes.Select(s => s.ToHtml()).ToArray());
    }

    public override string ToBbCode()
    {
        return string.Concat(SubNodes.Select(s => s.ToBbCode()).ToArray());
    }

    public override string ToText()
    {
        return string.Concat(SubNodes.Select(s => s.ToText()).ToArray());
    }

    public override SyntaxTreeNode SetSubNodes(IEnumerable<SyntaxTreeNode> subNodes)
    {
        ArgumentNullException.ThrowIfNull(subNodes);
        return new SequenceNode(subNodes);
    }

    internal override SyntaxTreeNode AcceptVisitor(SyntaxTreeVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.Visit(this);
    }

    protected override bool EqualsCore(SyntaxTreeNode b)
    {
        return true;
    }
}
