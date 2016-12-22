using Markdig.Renderers;

namespace Hikkaba.Service.MarkdigAddons
{
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
