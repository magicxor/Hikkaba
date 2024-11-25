namespace CodeKicker.BbCode;

public enum BbTagClosingStyle
{
    RequiresClosingTag = 0,
    AutoCloseElement = 1,
    LeafElementWithoutContent = 2, //leaf elements have no content - they are closed immediately
}