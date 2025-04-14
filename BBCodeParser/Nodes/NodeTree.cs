using System.Text;
using BBCodeParser.Telemetry;

namespace BBCodeParser.Nodes;

public class NodeTree : Node
{
    private readonly IReadOnlyDictionary<string, string> _securitySubstitutions;
    private readonly IReadOnlyDictionary<string, string> _aliasSubstitutions;

    public NodeTree(
        IReadOnlyDictionary<string, string> securitySubstitutions,
        IReadOnlyDictionary<string, string> aliasSubstitutions
    )
    {
        _securitySubstitutions = securitySubstitutions;
        _aliasSubstitutions = aliasSubstitutions;
        ChildNodes = new List<Node>();
        ParentNode = null;
    }

    public string ToHtml(
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        using var activity = BBCodeParserTelemetry.RendererSource.StartActivity();
        return ToHtml(_securitySubstitutions, _aliasSubstitutions, filter, filterAttributeValue);
    }

    public override string ToHtml(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        IReadOnlyDictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        var result = new StringBuilder(ChildNodes?.Count ?? 0);
        foreach (var childNode in ChildNodes?.Where(n => filter == null || filter(n)) ?? [])
        {
            result.Append(childNode.ToHtml(
                securitySubstitutions,
                aliasSubstitutions,
                filter,
                filterAttributeValue));
        }

        return result.ToString();
    }

    public string ToText(
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        using var activity = BBCodeParserTelemetry.RendererSource.StartActivity();
        return ToText(_securitySubstitutions, _aliasSubstitutions, filter, filterAttributeValue);
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
            result.Append(childNode.ToText(
                securitySubstitutions,
                aliasSubstitutions,
                filter,
                filterAttributeValue));
        }

        return result.ToString();
    }

    public string ToBb(
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        using var activity = BBCodeParserTelemetry.RendererSource.StartActivity();
        return ToBb(null, filter, filterAttributeValue);
    }

    public override string ToBb(
        IReadOnlyDictionary<string, string>? securitySubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        var result = new StringBuilder(ChildNodes?.Count ?? 0);
        foreach (var childNode in ChildNodes?.Where(n => filter == null || filter(n)) ?? [])
        {
            result.Append(childNode.ToBb(securitySubstitutions, filter, filterAttributeValue));
        }

        return result.ToString();
    }
}
