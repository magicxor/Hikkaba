using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeKicker.BbCode.SyntaxTree;

public sealed class TagNode : SyntaxTreeNode
{
    public TagNode(BbTag tag)
        : this(tag, null)
    {
    }

    public TagNode(BbTag tag, IEnumerable<SyntaxTreeNode> subNodes)
        : base(subNodes)
    {
        ArgumentNullException.ThrowIfNull(tag);
        Tag = tag;
        AttributeValues = new Dictionary<BbAttribute, string>();
    }

    public BbTag Tag { get; private set; }
    public IDictionary<BbAttribute, string> AttributeValues { get; private set; }

    public override string ToHtml()
    {
        var content = GetContent();
        return ReplaceAttributeValues(Tag.OpenTagTemplate, content) + (Tag.AutoRenderContent ? content : null) + ReplaceAttributeValues(Tag.CloseTagTemplate, content);
    }

    public override string ToBbCode()
    {
        var content = string.Concat(SubNodes.Select(s => s.ToBbCode()).ToArray());

        var attrs = "";
        var defAttr = Tag.FindAttribute("");
        if (defAttr != null)
        {
            if (AttributeValues.TryGetValue(defAttr, out string value))
            {
                attrs += "=" + value;
            }
        }
        foreach (var attrKvp in AttributeValues)
        {
            if (attrKvp.Key.Name == "")
            {
                continue;
            }

            attrs += " " + attrKvp.Key.Name + "=" + attrKvp.Value;
        }
        return "[" + Tag.Name + attrs + "]" + content + "[/" + Tag.Name + "]";
    }

    public override string ToText()
    {
        return string.Concat(SubNodes.Select(s => s.ToText()).ToArray());
    }

    private string GetContent()
    {
        var content = string.Concat(SubNodes.Select(s => s.ToHtml()).ToArray());
        return Tag.ContentTransformer == null ? content : Tag.ContentTransformer(content);
    }

    private string ReplaceAttributeValues(string template, string content)
    {
        var attributesWithValues = (from attr in Tag.Attributes
            group attr by attr.Id into gAttrByID
            let val = (from attr in gAttrByID
                let val = TryGetValue(attr)
                where val != null
                select new { attr, val }).FirstOrDefault()
            select new { attrID = gAttrByID.Key, attrAndVal = val }).ToList();

        var attrValuesById = attributesWithValues.Where(x => x.attrAndVal != null).ToDictionary(x => x.attrID, x => x.attrAndVal.val);
        attrValuesById.TryAdd(BbTag.ContentPlaceholderName, content);
        var output = template;
        foreach (var x in attributesWithValues)
        {
            var placeholderStr = "${" + x.attrID + "}";

            if (x.attrAndVal != null)
            {
                //replace attributes with values
                var rawValue = x.attrAndVal.val;
                var attribute = x.attrAndVal.attr;
                output = ReplaceAttribute(output, attribute, rawValue, placeholderStr, attrValuesById);
            }
        }

        //replace empty attributes
        var attributeIDsWithValues = new HashSet<string>(attributesWithValues.Where(x => x.attrAndVal != null).Select(x => x.attrID));
        var emptyAttributes = Tag.Attributes.Where(attr => !attributeIDsWithValues.Contains(attr.Id)).ToList();

        foreach (var attr in emptyAttributes)
        {
            var placeholderStr = "${" + attr.Id + "}";
            output = ReplaceAttribute(output, attr, null, placeholderStr, attrValuesById);
        }

        output = output.Replace("${" + BbTag.ContentPlaceholderName + "}", content);
        return output;
    }

    private static string ReplaceAttribute(string output, BbAttribute attribute, string rawValue, string placeholderStr, Dictionary<string, string> attrValuesById)
    {
        string effectiveValue;
        if (attribute.ContentTransformer == null)
        {
            effectiveValue = rawValue;
        }
        else
        {
            var ctx = new AttributeRenderingContextImpl(attribute, rawValue, attrValuesById);
            effectiveValue = attribute.ContentTransformer(ctx);
        }

        if (effectiveValue == null)
        {
            effectiveValue = "";
        }

        var encodedValue =
            attribute.HtmlEncodingMode == HtmlEncodingMode.HtmlAttributeEncode ? HttpUtility.HtmlAttributeEncode(effectiveValue)
            : attribute.HtmlEncodingMode == HtmlEncodingMode.HtmlEncode ? HttpUtility.HtmlEncode(effectiveValue)
            : effectiveValue;
        output = output.Replace(placeholderStr, encodedValue);
        return output;
    }

    private string TryGetValue(BbAttribute attr)
    {
        string val;
        AttributeValues.TryGetValue(attr, out val);
        return val;
    }

    public override SyntaxTreeNode SetSubNodes(IEnumerable<SyntaxTreeNode> subNodes)
    {
        ArgumentNullException.ThrowIfNull(subNodes);
        return new TagNode(Tag, subNodes)
        {
            AttributeValues = new Dictionary<BbAttribute, string>(AttributeValues),
        };
    }

    internal override SyntaxTreeNode AcceptVisitor(SyntaxTreeVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.Visit(this);
    }

    protected override bool EqualsCore(SyntaxTreeNode b)
    {
        var casted = (TagNode)b;
        return
            Tag == casted.Tag &&
            AttributeValues.All(attr => casted.AttributeValues[attr.Key] == attr.Value) &&
            casted.AttributeValues.All(attr => AttributeValues[attr.Key] == attr.Value);
    }

    private class AttributeRenderingContextImpl : IAttributeRenderingContext
    {
        public AttributeRenderingContextImpl(BbAttribute attribute, string attributeValue, IDictionary<string, string> getAttributeValueByIdData)
        {
            Attribute = attribute;
            AttributeValue = attributeValue;
            GetAttributeValueByIdData = getAttributeValueByIdData;
        }

        public BbAttribute Attribute { get; private set; }
        public string AttributeValue { get; private set; }
        public IDictionary<string, string> GetAttributeValueByIdData { get; private set; }

        public string GetAttributeValueById(string id)
        {
            string value;
            if (!GetAttributeValueByIdData.TryGetValue(id, out value))
            {
                return null;
            }

            return value;
        }
    }
}
