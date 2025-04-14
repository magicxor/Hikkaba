using System.Globalization;
using System.Text;
using BBCodeParser.Tags;

namespace BBCodeParser.Nodes;

public class TagNode : Node
{
    public string? AttributeValue { get; }
    public Tag? Tag { get; }

    public TagNode(Tag? tag, Node? parent, string? attributeValue)
    {
        Tag = tag;
        AttributeValue = attributeValue;
        ChildNodes = [];
        ParentNode = parent;
    }

    public override string ToHtml(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        IReadOnlyDictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        var attributeValue = filterAttributeValue == null
            ? AttributeValue
            : filterAttributeValue(this, AttributeValue);
        var result = new StringBuilder(Tag?.GetOpenHtml(attributeValue ?? string.Empty), (ChildNodes?.Count ?? 0) + 2);
        foreach (var childNode in ChildNodes?.Where(n => filter == null || filter(n)) ?? [])
        {
            if (Tag is CodeTag)
            {
                result.Append(childNode.ToBb(securitySubstitutions));
            }
            else if (Tag is PreformattedTag)
            {
                result.Append(childNode.ToHtml(securitySubstitutions, null, filter));
            }
            else if (Tag is not ListTag || childNode is not TextNode)
            {
                result.Append(childNode.ToHtml(securitySubstitutions, aliasSubstitutions, filter));
            }
        }

        if (Tag?.RequiresClosing == true)
        {
            result.Append(Tag.GetCloseHtml(attributeValue ?? string.Empty));
        }

        return result.ToString();
    }

    public override string ToText(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        IReadOnlyDictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null
    )
    {
        var result = new StringBuilder(ChildNodes?.Count ?? 0);
        foreach (var childNode in ChildNodes?.Where(n => filter == null || filter(n)) ?? [])
        {
            if (Tag is CodeTag)
            {
                result.Append(childNode.ToBb(securitySubstitutions, filter, filterAttributeValue));
            }
            else if (Tag is PreformattedTag)
            {
                result.Append(childNode.ToText(securitySubstitutions, null, filter, filterAttributeValue));
            }
            else
            {
                result.Append(childNode.ToText(securitySubstitutions, aliasSubstitutions, filter, filterAttributeValue));
            }
        }

        return result.ToString();
    }

    public override string ToBb(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        var attributeValue = filterAttributeValue == null
            ? AttributeValue
            : filterAttributeValue(this, AttributeValue);
        var result = new StringBuilder((Tag?.WithAttribute == true) && !string.IsNullOrEmpty(attributeValue)
            ? $"""[{Tag.Name}="{attributeValue}"]"""
            : $"[{Tag?.Name}]", (ChildNodes?.Count ?? 0) + 2);
        foreach (var childNode in ChildNodes?.Where(n => filter == null || filter(n)) ?? [])
        {
            result.Append(childNode.ToBb(securitySubstitutions, filter, filterAttributeValue));
        }

        if (Tag?.RequiresClosing == true)
        {
            result.Append(CultureInfo.InvariantCulture, $"[/{Tag.Name}]");
        }

        return result.ToString();
    }
}
