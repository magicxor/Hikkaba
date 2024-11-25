namespace CodeKicker.BbCode;

public interface IAttributeRenderingContext
{
    BbAttribute Attribute { get; }
    string AttributeValue { get; }
    string GetAttributeValueById(string id);
}