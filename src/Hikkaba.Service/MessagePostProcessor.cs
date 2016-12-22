using System;
using System.IO;
using System.Text;
using Hikkaba.Service.MarkdigAddons;
using Hikkaba.Service.MarkdigAddons.Blocks;
using Hikkaba.Service.MarkdigAddons.Parsers;
using Hikkaba.Service.MarkdigAddons.Renderers;
using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Hikkaba.Service
{
    public interface IMessagePostProcessor
    {
        string Process(string message);
    }

    // todo: add spoilers; insert <br> between spans
    public class MessagePostProcessor: IMessagePostProcessor
    {
        private readonly StringBuilder _stringBuilder;
        private readonly HtmlRenderer _htmlRenderer;
        private readonly MarkdownPipeline _markdownPipeline;
        
        public MessagePostProcessor()
        {
            _stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(_stringBuilder);
            _htmlRenderer = new HtmlRenderer(stringWriter);
            _htmlRenderer.ReplaceRenderer<ParagraphRenderer, BrParagraphRenderer>();

            var pipelineBuilder = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .UseSoftlineBreakAsHardlineBreak()
                .UseEmphasisExtras()
                .ReplaceBlockParser<ParagraphBlockParser, NoSetextParagraphBlockParser>() // disable setext headers
                .DisableHtml()                                 // due to security reasons
                .DisableBlockParser<ThematicBreakParser>()     // it's overkill for imageboard
                .DisableBlockParser<ListBlockParser>()         // it's overkill for imageboard
                .DisableBlockParser<IndentedCodeBlockParser>() // it's overkill for imageboard
                .DisableBlockParser<QuoteBlockParser>()        // replaced by SingleQuoteBlockParser
                .DisableBlockParser<HeadingBlockParser>()      // it's overkill for imageboard
                .DisableInlineParser<LinkInlineParser>();      // due to security reasons: no external pictures and javascript: links
            pipelineBuilder.DocumentProcessed += OnDocumentProcessed;
            _markdownPipeline = pipelineBuilder.Build();
        }

        private void OnDocumentProcessed(MarkdownDocument document)
        {
            ProcessTree(document);
        }

        private void ProcessTree(MarkdownObject markdownObject)
        {
            Type previousDescendantType = null;
            foreach (MarkdownObject child in markdownObject.Descendants())
            {
                ProcessNode(child, previousDescendantType);
                if (!(child is LiteralInline))
                {
                    previousDescendantType = child.GetType();
                }
            }
            foreach (MarkdownObject child in markdownObject.Descendants())
            {
                ProcessTree(child);
            }
        }

        private void ProcessNode(MarkdownObject markdownObject, Type previousType)
        {
            if ((previousType != null) && (markdownObject is SingleQuoteBlock) && (markdownObject.GetType() == previousType))
            {
                ((SingleQuoteBlock) markdownObject).Inline.InsertBefore(new LineBreakInline() { IsHard = true });
            }
        }

        public string Process(string message)
        {
            var parsedMessage = Markdown.Convert(message, _htmlRenderer, _markdownPipeline);            
            var result = parsedMessage.ToString().Trim();
            _stringBuilder.Clear();
            return result;
        }
    }
}
