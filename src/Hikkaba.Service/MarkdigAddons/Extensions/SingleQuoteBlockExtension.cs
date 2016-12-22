using Hikkaba.Service.MarkdigAddons.Parsers;
using Hikkaba.Service.MarkdigAddons.Renderers;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;

namespace Hikkaba.Service.MarkdigAddons.Extensions
{
    public class SingleQuoteBlockExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<SingleQuoteBlockParser>())
            {
                // The SingleQuoteBlockParser must come after the HeadingBlockParser
                if (pipeline.BlockParsers.Contains<HeadingBlockParser>())
                {
                    pipeline.BlockParsers.InsertBefore<HeadingBlockParser>(new SingleQuoteBlockParser());
                }
                else
                {
                    pipeline.BlockParsers.Insert(0, new SingleQuoteBlockParser());
                }
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var htmlRenderer = renderer as HtmlRenderer;
            htmlRenderer?.ObjectRenderers.AddIfNotAlready<SingleQuoteBlockRenderer>();
        }
    }
}
