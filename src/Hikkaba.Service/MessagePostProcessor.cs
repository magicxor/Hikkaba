using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Hikkaba.Common.Extensions;
using Hikkaba.Service.Extensions;
using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Hikkaba.Service
{
    public interface IMessagePostProcessor
    {
        string Process(string message);
    }

    // todo: add spoilers; blockquote should be one-line
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
            _htmlRenderer.ReplaceRenderer<HeadingRenderer, PlainTextHeadingRenderer>();
            _htmlRenderer.ReplaceRenderer<QuoteBlockRenderer, WakabaStyleQuoteBlockRenderer>();

            var pipelineBuilder = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .UseSoftlineBreakAsHardlineBreak()
                .UseEmphasisExtras()
                .DisableHtml() // due to security reasons
                .DisableBlockParser<ThematicBreakParser>() // it's overkill for imageboard
                .DisableBlockParser<ListBlockParser>()
                .DisableBlockParser<IndentedCodeBlockParser>()
                .DisableInlineParser<LinkInlineParser>(); // due to security reasons: no external pictures and javascript: links
            _markdownPipeline = pipelineBuilder.Build();
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
