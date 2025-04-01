using BBCodeParser.Nodes;
using BBCodeParser.Tags;

namespace BBCodeParser;

public interface IBBParser
{
    NodeTree Parse(string input);
    Tag[] GetTags();
}