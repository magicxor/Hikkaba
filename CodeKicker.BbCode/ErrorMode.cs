namespace CodeKicker.BbCode;

public enum ErrorMode
{
    /// <summary>
    /// Every syntax error throws a BBCodeParsingException.
    /// </summary>
    Strict,

    /// <summary>
    /// Syntax errors with obvious meaning will be corrected automatically.
    /// </summary>
    TryErrorCorrection,

    /// <summary>
    /// The parser will never throw an exception. Invalid tags like "array[0]" will be interpreted as text.
    /// </summary>
    ErrorFree,
}