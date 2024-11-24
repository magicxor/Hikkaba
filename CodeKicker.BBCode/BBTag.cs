using System;

namespace CodeKicker.BBCode;

public class BbTag
{
    public const string ContentPlaceholderName = "content";

    public BbTag(string name, string openTagTemplate, string closeTagTemplate, bool autoRenderContent, BbTagClosingStyle tagClosingClosingStyle, Func<string, string> contentTransformer, bool enableIterationElementBehavior, params BbAttribute[] attributes)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(openTagTemplate);
        ArgumentNullException.ThrowIfNull(closeTagTemplate);
        if (!Enum.IsDefined(tagClosingClosingStyle)) throw new ArgumentOutOfRangeException(nameof(tagClosingClosingStyle));

        Name = name;
        OpenTagTemplate = openTagTemplate;
        CloseTagTemplate = closeTagTemplate;
        AutoRenderContent = autoRenderContent;
        TagClosingStyle = tagClosingClosingStyle;
        ContentTransformer = contentTransformer;
        EnableIterationElementBehavior = enableIterationElementBehavior;
        Attributes = attributes ?? [];
    }

    public BbTag(string name, string openTagTemplate, string closeTagTemplate, bool autoRenderContent, BbTagClosingStyle tagClosingClosingStyle, Func<string, string> contentTransformer, params BbAttribute[] attributes)
        : this(name, openTagTemplate, closeTagTemplate, autoRenderContent, tagClosingClosingStyle, contentTransformer, false, attributes)
    {
    }

    public BbTag(string name, string openTagTemplate, string closeTagTemplate, bool autoRenderContent, bool requireClosingTag, Func<string, string> contentTransformer, params BbAttribute[] attributes)
        : this(name, openTagTemplate, closeTagTemplate, autoRenderContent, requireClosingTag ? BbTagClosingStyle.RequiresClosingTag : BbTagClosingStyle.AutoCloseElement, contentTransformer, attributes)
    {
    }

    public BbTag(string name, string openTagTemplate, string closeTagTemplate, bool autoRenderContent, bool requireClosingTag, params BbAttribute[] attributes)
        : this(name, openTagTemplate, closeTagTemplate, autoRenderContent, requireClosingTag, null, attributes)
    {
    }

    public BbTag(string name, string openTagTemplate, string closeTagTemplate, params BbAttribute[] attributes)
        : this(name, openTagTemplate, closeTagTemplate, true, true, attributes)
    {
    }

    public string Name { get; private set; }
    public string OpenTagTemplate { get; private set; }
    public string CloseTagTemplate { get; private set; }
    public bool AutoRenderContent { get; private set; }
    public bool StopProcessing { get; set; }
    public bool EnableIterationElementBehavior { get; private set; }
    public bool RequiresClosingTag
    {
        get { return TagClosingStyle == BbTagClosingStyle.RequiresClosingTag; }
    }
    public BbTagClosingStyle TagClosingStyle { get; private set; }
    public Func<string, string> ContentTransformer { get; private set; } //allows for custom modification of the tag content before rendering takes place
    public BbAttribute[] Attributes { get; private set; }

    public BbAttribute FindAttribute(string name)
    {
        return Array.Find(Attributes, a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}

public enum BbTagClosingStyle
{
    RequiresClosingTag = 0,
    AutoCloseElement = 1,
    LeafElementWithoutContent = 2, //leaf elements have no content - they are closed immediately
}

public enum HtmlEncodingMode
{
    HtmlEncode = 0,
    HtmlAttributeEncode = 1,
    UnsafeDontEncode = 2,
}
