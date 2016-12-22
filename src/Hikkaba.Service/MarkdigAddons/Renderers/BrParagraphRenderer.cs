using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Hikkaba.Service.MarkdigAddons.Renderers
{
    public class BrParagraphRenderer : ParagraphRenderer
    {
        protected override void Write(HtmlRenderer renderer, ParagraphBlock obj)
        {
            if (!renderer.ImplicitParagraph)
            {
                if (!renderer.IsFirstInContainer)
                {
                    renderer.EnsureLine();
                    renderer.Write("<br/>");
                }
            }
            renderer.WriteLeafInline(obj);
            if (!renderer.ImplicitParagraph)
            {
                if (!renderer.IsLastInContainer)
                {
                    renderer.WriteLine("<br/>");
                }
            }
        }
    }
}
