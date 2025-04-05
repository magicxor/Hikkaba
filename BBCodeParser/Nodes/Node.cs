namespace BBCodeParser.Nodes;

public abstract class Node
{
    protected List<Node>? ChildNodes { get; set; }
    public Node? ParentNode { get; set; }

    public abstract string ToHtml(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        IReadOnlyDictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null);

    public abstract string ToText(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        IReadOnlyDictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null);

    public abstract string ToBb(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null);

    public virtual void AddChild(Node node)
    {
        ChildNodes?.Add(node);
    }
}
