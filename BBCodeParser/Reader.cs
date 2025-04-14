using System.Text.RegularExpressions;
using BBCodeParser.Tags;

namespace BBCodeParser;

public partial class Reader
{
    private readonly Regex _bbPattern = BbPatternRegex();
    private readonly string _input;
    private readonly Tag[] _tags;

    private int _position;
    private Match _match;

    public Reader(string input, Tag[] tags)
    {
        _input = input;
        _tags = tags;
        _position = 0;
        _match = _bbPattern.Match(_input);
    }

    public bool TryRead(out TagResult? result)
    {
        if (_position == _input.Length)
        {
            result = null;
            return false;
        }

        if (_match.Success)
        {
            var tagName = _match.Groups["tag"].Value;
            var matchingTag = _tags.FirstOrDefault(t => t.Name == tagName);

            if (matchingTag == null)
            {
                result = new TagResult
                {
                    Text = _input.Substring(_position, _match.Index + _match.Length - _position),
                    TagType = TagType.NoResult,
                };
            }
            else
            {
                result = new TagResult
                {
                    Match = _match.Value,
                    Text = _input.Substring(_position, _match.Index - _position),
                    Tag = matchingTag,
                    AttributeValue = _match.Groups["value"].Value,
                    TagType = _match.Groups["closing"].Success ? TagType.Close : TagType.Open,
                };
            }

            _position = _match.Index + _match.Length;
            _match = _match.NextMatch();
            return true;
        }

        var inputSubstringPos = _input.Substring(_position);
        _position = _input.Length;

        result = new TagResult
        {
            Text = inputSubstringPos,
            TagType = TagType.NoResult,
        };

        return true;
    }

    [GeneratedRegex("""\[(?<closing>\/)?(?<tag>\w+)(\=\"(?<value>[^"\[\]]*?)\")?\]""", RegexOptions.Compiled)]
    private static partial Regex BbPatternRegex();
}
