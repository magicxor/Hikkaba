using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Footers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;

namespace Hikkaba.Service.Extensions.SingleQuoteBlock
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
