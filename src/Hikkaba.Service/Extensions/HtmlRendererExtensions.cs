using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Hikkaba.Service.Extensions
{
    public class PlainTextHeadingRenderer : HeadingRenderer
    {
        protected override void Write(HtmlRenderer renderer, HeadingBlock obj)
        {
            renderer.WriteLeafInline(obj);
            renderer.WriteLine();
        }
    }

    public class WakabaStyleQuoteBlockRenderer : QuoteBlockRenderer
    {
        protected override void Write(HtmlRenderer renderer, QuoteBlock obj)
        {
            renderer.EnsureLine();
            renderer.Write("<span class=\"text-success\"").WriteAttributes((MarkdownObject)obj).WriteLine(">");
            bool implicitParagraph = renderer.ImplicitParagraph;
            renderer.ImplicitParagraph = false;
            renderer.WriteChildren((ContainerBlock)obj);
            renderer.ImplicitParagraph = implicitParagraph;
            renderer.WriteLine("</span>");
        }
    }

    public static class HtmlRendererExtensions
    {
        public static void ReplaceRenderer<TOldRenderer, TNewRenderer>(this HtmlRenderer htmlRenderer) 
            where TOldRenderer : IMarkdownObjectRenderer 
            where TNewRenderer : TOldRenderer, new()
        {
            var oldRenderer = htmlRenderer.ObjectRenderers.Find<TOldRenderer>();
            if (oldRenderer != null)
            {
                htmlRenderer.ObjectRenderers.Remove(oldRenderer);
            }
            TOldRenderer newRenderer = new TNewRenderer();
            htmlRenderer.ObjectRenderers.AddIfNotAlready<TOldRenderer>(newRenderer);
        }
    }
}
