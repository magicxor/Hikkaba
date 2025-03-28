using System.Text.RegularExpressions;
using BBCodeParser.Tags;

namespace BBCodeParser;

public partial class Reader
{
    private readonly string _input;
    private readonly Tag[] _tags;
    private int _position;
    private Match _match;
    private readonly Regex _bbPattern = BbPatternRegex();

    public Reader(string input, Tag[] tags)
    {
        this._input = input;
        this._tags = tags;
        _position = 0;
        _match = _bbPattern.Match(this._input);
    }

    public bool TryRead(out BBCodeParser.Tags.TagResult? result)
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
                result = new BBCodeParser.Tags.TagResult
                {
                    Text = _input.Substring(_position, _match.Index + _match.Length - _position),
                    TagType = BBCodeParser.Tags.TagType.NoResult,
                };
            }
            else
            {
                result = new BBCodeParser.Tags.TagResult
                {
                    Match = _match.Value,
                    Text = _input.Substring(_position, _match.Index - _position),
                    Tag = matchingTag,
                    AttributeValue = _match.Groups["value"].Value,
                    TagType = _match.Groups["closing"].Success ? BBCodeParser.Tags.TagType.Close : BBCodeParser.Tags.TagType.Open,
                };
            }

            _position = _match.Index + _match.Length;
            _match = _match.NextMatch();
            return true;
        }

        var inputSubstringPos = _input.Substring(_position);
        _position = _input.Length;

        result = new BBCodeParser.Tags.TagResult
        {
            Text = inputSubstringPos,
            TagType = BBCodeParser.Tags.TagType.NoResult,
        };

        return true;
    }

    [GeneratedRegex("""\[(?<closing>\/)?(?<tag>\w+)(\=\"(?<value>[^"\[\]]*?)\")?\]""", RegexOptions.Compiled)]
    private static partial Regex BbPatternRegex();
}
