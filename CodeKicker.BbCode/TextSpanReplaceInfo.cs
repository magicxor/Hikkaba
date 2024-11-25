using System;
using CodeKicker.BbCode.SyntaxTree;

namespace CodeKicker.BbCode;

public class TextSpanReplaceInfo
{
    public TextSpanReplaceInfo(int index, int length, SyntaxTreeNode replacement)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Index = index;
        Length = length;
        Replacement = replacement;
    }

    public int Index { get; private set; }
    public int Length { get; private set; }
    public SyntaxTreeNode Replacement { get; private set; }
}