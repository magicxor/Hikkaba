using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Hikkaba.Service.Extensions
{
    public class PlainTextHeadingRenderer : HeadingRenderer
    {
        protected override void Write(HtmlRenderer renderer, HeadingBlock obj)
        {
            renderer.WriteLeafInline(obj);
            renderer.WriteLine("<br />");
        }
    }

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

    public static class HtmlRendererExtensions
    {
        public static void ReplaceRenderer<TOldRenderer, TNewRenderer>(this HtmlRenderer htmlRenderer)
            where TOldRenderer : IMarkdownObjectRenderer
            where TNewRenderer : TOldRenderer, new()
        {
            TOldRenderer newRenderer = new TNewRenderer();
            ReplaceRenderer<TOldRenderer>(htmlRenderer, newRenderer);
        }

        public static void ReplaceRenderer<TRenderer>(this HtmlRenderer htmlRenderer, TRenderer newRenderer)
            where TRenderer : IMarkdownObjectRenderer
        {
            var oldRenderer = htmlRenderer.ObjectRenderers.Find<TRenderer>();
            if (oldRenderer != null)
            {
                htmlRenderer.ObjectRenderers.Remove(oldRenderer);
            }
            htmlRenderer.ObjectRenderers.AddIfNotAlready<TRenderer>(newRenderer);
        }
    }
}
