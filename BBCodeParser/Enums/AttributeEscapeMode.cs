namespace BBCodeParser.Enums;

public enum AttributeEscapeMode
{
    /// <summary>
    /// Use this mode to escape only characters that are interpreted as HTML control characters (&lt;, &gt;, &amp;).
    /// </summary>
    Html,

    /// <summary>
    /// Use this mode to escape characters that are interpreted as HTML control characters (&lt;, &gt;, &amp;) and
    /// potentially dangerous characters (&quot;, &apos;, `, etc.) and strings (javascript:, etc.).
    /// </summary>
    JsXss,

    /// <summary>
    /// Use this mode to escape strings that should contain a valid absolute URI.
    /// </summary>
    AbsoluteUri,

    /// <summary>
    /// Use this mode to escape strings that should contain a valid relative URI.
    /// </summary>
    RelativeUri,
}
