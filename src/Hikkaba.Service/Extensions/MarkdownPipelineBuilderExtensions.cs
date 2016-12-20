using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;

namespace Hikkaba.Service.Extensions
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder DisableBlockParser<T>(this MarkdownPipelineBuilder pipeline) where T : BlockParser
        {
            var blockParser = pipeline.BlockParsers.Find<T>();
            if (blockParser != null)
            {
                pipeline.BlockParsers.Remove(blockParser);
            }
            return pipeline;
        }

        public static MarkdownPipelineBuilder DisableInlineParser<T>(this MarkdownPipelineBuilder pipeline) where T : InlineParser
        {
            var inlineParser = pipeline.InlineParsers.Find<T>();
            if (inlineParser != null)
            {
                pipeline.InlineParsers.Remove(inlineParser);
            }
            return pipeline;
        }
    }
}
