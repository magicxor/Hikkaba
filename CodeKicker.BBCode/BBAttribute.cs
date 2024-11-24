using System;
using System.Diagnostics.CodeAnalysis;

namespace CodeKicker.BBCode;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
public class BbAttribute
{
    public BbAttribute(string id, string name)
        : this(id, name, null, HtmlEncodingMode.HtmlAttributeEncode)
    {
    }
    public BbAttribute(string id, string name, Func<IAttributeRenderingContext, string> contentTransformer)
        : this(id, name, contentTransformer, HtmlEncodingMode.HtmlAttributeEncode)
    {
    }
    public BbAttribute(string id, string name, Func<IAttributeRenderingContext, string> contentTransformer, HtmlEncodingMode htmlEncodingMode)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        if (!Enum.IsDefined(htmlEncodingMode)) throw new ArgumentOutOfRangeException(nameof(htmlEncodingMode));

        Id = id;
        Name = name;
        ContentTransformer = contentTransformer;
        HtmlEncodingMode = htmlEncodingMode;
    }

    public string Id { get; private set; } //ID is used to reference the attribute value
    public string Name { get; private set; } //Name is used during parsing
    public Func<IAttributeRenderingContext, string> ContentTransformer { get; private set; } //allows for custom modification of the attribute value before rendering takes place
    public HtmlEncodingMode HtmlEncodingMode { get; set; }

    public static Func<IAttributeRenderingContext, string> AdaptLegacyContentTransformer(Func<string, string> contentTransformer)
    {
        return contentTransformer == null ? (Func<IAttributeRenderingContext, string>)null : ctx => contentTransformer(ctx.AttributeValue);
    }
}
public interface IAttributeRenderingContext
{
    BbAttribute Attribute { get; }
    string AttributeValue { get; }
    string GetAttributeValueById(string id);
}
