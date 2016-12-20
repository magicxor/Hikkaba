using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Hikkaba.Common.Extensions;
using Hikkaba.Service.Extensions;
using Markdig;
using Markdig.Helpers;
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

    public class MessagePostProcessor: IMessagePostProcessor
    {
        public string Process(string message)
        {
            var pipeline = new MarkdownPipelineBuilder()
                            .DisableHtml()
                            .DisableHeadings()
                            .UseAutoLinks()
                            .UseEmphasisExtras()
                            .Build();
            var parsedMessage = Markdown.Parse(message, pipeline);
            ProcessNode(parsedMessage);

            var builder = new StringBuilder();
            var textwriter = new StringWriter(builder);
            var renderer = new HtmlRenderer(textwriter);
            renderer.Render(parsedMessage);

            var result = builder.ToString().Trim();
            return result;
        }

        static void ProcessNode(MarkdownObject markdownObject)
        {
            foreach (MarkdownObject child in markdownObject.Descendants())
            {
                // LinkInline can be both an image or a <a href="...">
                LinkInline link = child as LinkInline;
                if (link != null)
                {
                    if ((link.IsImage)
                    || !(link.Url.StartsWith("http://") || link.Url.StartsWith("https://") || link.Url.StartsWith("ftp://")))
                    {
                        link.Url = "#";
                    }
                }
                ProcessNode(child);
            }
        }
    }
}
