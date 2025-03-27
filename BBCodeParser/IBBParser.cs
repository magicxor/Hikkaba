using BBCodeParser.Nodes;
using BBCodeParser.Tags;

namespace BBCodeParser;

public interface IBbParser
{
    NodeTree Parse(string input);
    Tag[] GetTags();
}