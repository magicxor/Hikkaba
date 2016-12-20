using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Markdig.Parsers;

namespace Hikkaba.Service.Extensions
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder DisableHeadings(this MarkdownPipelineBuilder pipeline)
        {
            var headingBlockParser = pipeline.BlockParsers.Find<HeadingBlockParser>();
            if (headingBlockParser != null)
            {
                pipeline.BlockParsers.Remove(headingBlockParser);
            }
            return pipeline;
        }
    }
}
