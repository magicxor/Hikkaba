using Markdig.Renderers;
using Markdig.Renderers.Html;
using Hikkaba.Service.MarkdigAddons.Blocks;

namespace Hikkaba.Service.MarkdigAddons.Renderers
{
    public class SingleQuoteBlockRenderer : HtmlObjectRenderer<SingleQuoteBlock>
    {
        protected override void Write(HtmlRenderer renderer, SingleQuoteBlock obj)
        {
            renderer.Write("<span class=\"text-success\">&gt; ");
            renderer.WriteLeafInline(obj);
            renderer.WriteLine("</span>");
        }
    }
}
