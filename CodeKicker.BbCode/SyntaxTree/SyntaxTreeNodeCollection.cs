using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeKicker.BbCode.SyntaxTree;

public class SyntaxTreeNodeCollection : Collection<SyntaxTreeNode>, ISyntaxTreeNodeCollection
{
    public SyntaxTreeNodeCollection()
        : base()
    {
    }

    public SyntaxTreeNodeCollection(IEnumerable<SyntaxTreeNode> list)
        : base(list.ToArray())
    {
        ArgumentNullException.ThrowIfNull(list);
    }

    protected override void SetItem(int index, SyntaxTreeNode item)
    {
        ArgumentNullException.ThrowIfNull(item);
        base.SetItem(index, item);
    }

    protected override void InsertItem(int index, SyntaxTreeNode item)
    {
        ArgumentNullException.ThrowIfNull(item);
        base.InsertItem(index, item);
    }
}
