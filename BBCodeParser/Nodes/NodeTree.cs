using System.Text;
using BBCodeParser.Telemetry;

namespace BBCodeParser.Nodes;

public class NodeTree : Node
{
    private readonly Dictionary<string, string> _securitySubstitutions;
    private readonly Dictionary<string, string> _aliasSubstitutions;

    public NodeTree(
        Dictionary<string, string> securitySubstitutions,
        Dictionary<string, string> aliasSubstitutions
    )
    {
        this._securitySubstitutions = securitySubstitutions;
        this._aliasSubstitutions = aliasSubstitutions;
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

    public string ToText(
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        using var activity = BBCodeParserTelemetry.RendererSource.StartActivity();
        return ToText(_securitySubstitutions, _aliasSubstitutions, filter, filterAttributeValue);
    }

    public string ToBb(
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        using var activity = BBCodeParserTelemetry.RendererSource.StartActivity();
        return ToBb(null, filter, filterAttributeValue);
    }

    public override string ToHtml(
        Dictionary<string, string>? securitySubstitutions,
        Dictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null)
    {
        var result = new StringBuilder(ChildNodes?.Count ?? 0);
        foreach (var childNode in ChildNodes?.Where(n => filter == null || filter(n)) ?? [])
        {
            result.Append(childNode.ToHtml(securitySubstitutions, aliasSubstitutions, filter,
                filterAttributeValue));
        }

        return result.ToString();
    }

    public override string ToText(
        Dictionary<string, string>? securitySubstitutions,
        Dictionary<string, string>? aliasSubstitutions,
        Func<Node, bool>? filter = null,
        Func<Node, string?, string>? filterAttributeValue = null
    )
    {
        var result = new StringBuilder(ChildNodes?.Count ?? 0);
        foreach (var childNode in ChildNodes?.Where(n => filter == null || filter(n)) ?? [])
        {
            result.Append(childNode.ToText(securitySubstitutions, aliasSubstitutions, filter,
                filterAttributeValue));
        }

        return result.ToString();
    }

    public override string ToBb(
        Dictionary<string, string>? securitySubstitutions,
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
