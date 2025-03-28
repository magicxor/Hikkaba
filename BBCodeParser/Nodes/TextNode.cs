namespace BBCodeParser.Nodes;

public class TextNode : Node
{
    private readonly string _text;

    public TextNode(string text)
    {
        this._text = text;
    }

    private static string SubstituteText(string text, Dictionary<string, string>? substitutions)
    {
        return substitutions == null
            ? text
            : substitutions.Aggregate(text,
                (current, substitution) => current.Replace(substitution.Key, substitution.Value));
    }

    public override string ToHtml(
        Dictionary<string, string>? securitySubstitutions,
        Dictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        return SubstituteText(SubstituteText(_text, securitySubstitutions), aliasSubstitutions);
    }

    public override string ToText(
        Dictionary<string, string>? securitySubstitutions,
        Dictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null
    )
    {
        return SubstituteText(SubstituteText(_text, securitySubstitutions), aliasSubstitutions);
    }

    public override string ToBb(
        Dictionary<string, string>? securitySubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        return SubstituteText(_text, securitySubstitutions);
    }

    public override void AddChild(Node node)
    {
        throw new InvalidOperationException($"Cannot add a child to a {nameof(TextNode)}.");
    }
}
