using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Hikkaba.Service.MarkdigAddons.Renderers
{
    public class PlainTextParagraphRenderer : ParagraphRenderer
    {
        protected override void Write(HtmlRenderer renderer, ParagraphBlock obj)
        {
            if (!renderer.ImplicitParagraph)
            {
                if (!renderer.IsFirstInContainer)
                {
                    renderer.EnsureLine();
                }
            }
            renderer.WriteLeafInline(obj);
            if (!renderer.ImplicitParagraph)
            {
                if (!renderer.IsLastInContainer)
                {
                    renderer.WriteLine(Environment.NewLine);
                }
            }
        }
    }
}
