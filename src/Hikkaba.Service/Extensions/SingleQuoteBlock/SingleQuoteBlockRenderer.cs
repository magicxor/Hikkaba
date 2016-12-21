using System.Globalization;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Hikkaba.Service.Extensions.SingleQuoteBlock
{
    public class SingleQuoteBlockRenderer : HtmlObjectRenderer<SingleQuoteBlock>
    {
        protected override void Write(HtmlRenderer renderer, SingleQuoteBlock obj)
        {
            renderer.Write("<span class=\"text-success\">");
            renderer.WriteLeafInline(obj);
            renderer.WriteLine("</span>");
        }
    }
}
