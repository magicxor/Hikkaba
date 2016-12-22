using Hikkaba.Service.MarkdigAddons.Extensions;
using Markdig;
using Markdig.Parsers;

namespace Hikkaba.Service.MarkdigAddons
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder ReplaceInlineParser<TOldParser, TNewParser>(this MarkdownPipelineBuilder pipeline)
            where TOldParser : InlineParser
            where TNewParser : TOldParser, new()
        {
            TOldParser newParser = new TNewParser();
            return ReplaceInlineParser<TOldParser>(pipeline, newParser);
        }

        public static MarkdownPipelineBuilder ReplaceInlineParser<T>(this MarkdownPipelineBuilder pipeline, T newParser) where T : InlineParser
        {
            var inlineParser = pipeline.InlineParsers.Find<T>();
            if (inlineParser != null)
            {
                pipeline.InlineParsers.Remove(inlineParser);
            }
            pipeline.InlineParsers.AddIfNotAlready<T>(newParser);
            return pipeline;
        }

        public static MarkdownPipelineBuilder ReplaceBlockParser<TOldParser, TNewParser>(this MarkdownPipelineBuilder pipeline) 
            where TOldParser : BlockParser 
            where TNewParser : TOldParser, new()
        {
            TOldParser newParser = new TNewParser();
            return ReplaceBlockParser<TOldParser>(pipeline, newParser);
        }

        public static MarkdownPipelineBuilder ReplaceBlockParser<T>(this MarkdownPipelineBuilder pipeline, T newParser) where T : BlockParser
        {
            var blockParser = pipeline.BlockParsers.Find<T>();
            if (blockParser != null)
            {
                pipeline.BlockParsers.Remove(blockParser);
            }
            pipeline.BlockParsers.AddIfNotAlready<T>(newParser);
            return pipeline;
        }

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

        public static MarkdownPipelineBuilder UseSingleQuoteBlocks(this MarkdownPipelineBuilder pipeline)
        {
            pipeline.Extensions.AddIfNotAlready<SingleQuoteBlockExtension>();
            return pipeline;
        }
    }
}
