using System.Linq;
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

    public bool TryRead(out BBCodeParser.Tags.TagResult result)
    {
        if (_position == _input.Length)
        {
            result = null;
            return false;
        }

        result = new BBCodeParser.Tags.TagResult();
        if (_match.Success)
        {
            var tagName = _match.Groups["tag"].Value;
            var matchingTag = _tags.FirstOrDefault(t => t.Name == tagName);

            if (matchingTag == null)
            {
                result.Text = _input.Substring(_position, _match.Index + _match.Length - _position);
                result.TagType = BBCodeParser.Tags.TagType.NoResult;
            }
            else
            {
                result.Match = _match.Value;
                result.Text = _input.Substring(_position, _match.Index - _position);
                result.Tag = matchingTag;
                result.AttributeValue = _match.Groups["value"].Value;
                result.TagType = _match.Groups["closing"].Success ? BBCodeParser.Tags.TagType.Close : BBCodeParser.Tags.TagType.Open;
            }

            _position = _match.Index + _match.Length;
            _match = _match.NextMatch();
            return true;
        }

        result.Text = _input.Substring(_position);
        _position = _input.Length;
        result.TagType = BBCodeParser.Tags.TagType.NoResult;
        return true;
    }

    [GeneratedRegex("""\[(?<closing>\/)?(?<tag>\w+)(\=\"(?<value>[^"\[\]]*?)\")?\]""", RegexOptions.Compiled)]
    private static partial Regex BbPatternRegex();
}
