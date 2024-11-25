using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeKicker.BbCode.SyntaxTree;

public class ImmutableSyntaxTreeNodeCollection : ReadOnlyCollection<SyntaxTreeNode>, ISyntaxTreeNodeCollection
{
    public ImmutableSyntaxTreeNodeCollection(IEnumerable<SyntaxTreeNode> list)
        : base(list.ToArray())
    {
        ArgumentNullException.ThrowIfNull(list);
    }

    internal ImmutableSyntaxTreeNodeCollection(IList<SyntaxTreeNode> list, bool isFresh)
        : base(isFresh ? list : list.ToArray())
    {
    }
}
