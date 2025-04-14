namespace BBCodeParser.Nodes;

public class TextNode : Node
{
    private readonly string _text;

    public TextNode(string text)
    {
        _text = text;
    }

    private static string SubstituteText(string text, IReadOnlyDictionary<string, string>? substitutions)
    {
        return substitutions == null
            ? text
            : substitutions.Aggregate(text,
                (current, substitution) => current.Replace(substitution.Key, substitution.Value, StringComparison.OrdinalIgnoreCase));
    }

    public override string ToHtml(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        IReadOnlyDictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        return SubstituteText(SubstituteText(_text, securitySubstitutions), aliasSubstitutions);
    }

    public override string ToText(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        IReadOnlyDictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null
    )
    {
        return SubstituteText(SubstituteText(_text, securitySubstitutions), aliasSubstitutions);
    }

    public override string ToBb(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
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
