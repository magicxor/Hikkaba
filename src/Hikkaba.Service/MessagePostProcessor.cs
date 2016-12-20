using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
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
                            .Build();
            var parsedMessage = Markdown.Parse(message, pipeline);
            RemoveDangerousLinks(parsedMessage);

            var builder = new StringBuilder();
            var textwriter = new StringWriter(builder);
            var renderer = new HtmlRenderer(textwriter);
            renderer.Render(parsedMessage);

            var result = builder.ToString();
            return result.Trim();
        }

        static void RemoveDangerousLinks(MarkdownObject markdownObject)
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
                        link.Label = "Denied link";
                    }
                }
                RemoveDangerousLinks(child);
            }
        }
    }
}
