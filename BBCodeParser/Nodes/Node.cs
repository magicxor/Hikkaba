using System;
using System.Collections.Generic;

namespace BBCodeParser.Nodes;

public abstract class Node
{
    protected List<Node> ChildNodes { get; set; }
    public Node ParentNode { get; set; }

    public abstract string ToHtml(
        Dictionary<string, string> securitySubstitutions,
        Dictionary<string, string> aliasSubstitutions,
        Func<Node, bool> filter = null,
        Func<Node, string, string> filterAttributeValue = null);

    public abstract string ToText(
        Dictionary<string, string> securitySubstitutions,
        Dictionary<string, string> aliasSubstitutions,
        Func<Node, bool> filter = null,
        Func<Node, string, string> filterAttributeValue = null);

    public abstract string ToBb(
        Dictionary<string, string> securitySubstitutions,
        Func<Node, bool> filter = null,
        Func<Node, string, string> filterAttributeValue = null);

    public virtual void AddChild(Node node)
    {
        ChildNodes.Add(node);
    }
}