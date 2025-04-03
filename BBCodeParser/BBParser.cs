using BBCodeParser.Exceptions;
using BBCodeParser.Nodes;
using BBCodeParser.Tags;
using BBCodeParser.Telemetry;

namespace BBCodeParser;

public class BBParser : IBBParser
{
    private readonly Tag[] _tags;
    private readonly Dictionary<string, string> _securitySubstitutions;
    private readonly Dictionary<string, string> _aliasSubstitutions;
    private const int TreeMaxDepth = 6000;

    public static readonly Dictionary<string, string> SecuritySubstitutions = new()
    {
        {"&", "&amp;"},
        {"<", "&lt;"},
        {">", "&gt;"},
    };

    public BBParser(
        Tag[] tags,
        Dictionary<string, string> securitySubstitutions,
        Dictionary<string, string> aliasSubstitutions)
    {
        this._tags = tags;
        this._securitySubstitutions = securitySubstitutions;
        this._aliasSubstitutions = aliasSubstitutions;
    }

    public NodeTree Parse(string input)
    {
        using var activity = BBCodeParserTelemetry.ParserSource.StartActivity();

        var nodeTree = new NodeTree(_securitySubstitutions, _aliasSubstitutions);
        var treeDepth = 0;
        if (string.IsNullOrWhiteSpace(input))
        {
            nodeTree.AddChild(new TextNode(string.Empty));
            return nodeTree;
        }

        var reader = new Reader(input, _tags);
        var current = (Node) nodeTree;
        if (!reader.TryRead(out var tagResult))
        {
            current.AddChild(new TextNode(input));
        }
        else
        {
            do
            {
                if (!string.IsNullOrEmpty(tagResult?.Text))
                {
                    current?.AddChild(new TextNode(tagResult.Text));
                }

                // no parsing inside CodeTag
                var isInsideCodeTag = (current as TagNode)?.Tag is CodeTag;
                var resultIsClosingCodeTag = tagResult?.Tag is CodeTag && tagResult.TagType == BBCodeParser.Tags.TagType.Close;
                if (isInsideCodeTag && !resultIsClosingCodeTag && !string.IsNullOrEmpty(tagResult?.Match) &&
                    tagResult.Tag != null)
                {
                    current?.AddChild(new TextNode(tagResult.Match));
                    continue;
                }

                switch (tagResult?.TagType)
                {
                    case BBCodeParser.Tags.TagType.NoResult:
                        continue;
                    case BBCodeParser.Tags.TagType.Open:
                        var tagNode = new TagNode(tagResult.Tag, current, tagResult.AttributeValue);
                        current?.AddChild(tagNode);

                        if (tagResult.Tag?.RequiresClosing != true) continue;

                        current = tagNode;
                        treeDepth++;
                        if (treeDepth > TreeMaxDepth)
                        {
                            throw new BBParserException("Tree is too deep");
                        }

                        break;
                    default:
                        if (tagResult?.TagType == BBCodeParser.Tags.TagType.Close && current != nodeTree)
                        {
                            var currentTagNode = (TagNode?) current;
                            if (currentTagNode?.Tag?.Name != tagResult.Tag?.Name)
                            {
                                do
                                {
                                    current = current?.ParentNode;
                                    treeDepth--;
                                } while (current != nodeTree && currentTagNode?.Tag?.Name != tagResult?.Tag?.Name);
                            }
                            else
                            {
                                current = current?.ParentNode;
                                treeDepth--;
                            }
                        }

                        break;
                }
            } while (reader.TryRead(out tagResult));
        }

        return nodeTree;
    }

    public Tag[] GetTags()
    {
        return _tags;
    }
}
