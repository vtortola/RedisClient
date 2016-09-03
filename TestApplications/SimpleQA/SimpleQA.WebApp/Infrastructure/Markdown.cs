using SimpleQA.Markdown;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQA.WebApp.Infrastructure
{
    public class Markdown : IMarkdown
    {
        MarkdownSharp.Markdown _markdown;

        public Markdown()
        {
            _markdown = new MarkdownSharp.Markdown();
        }

        public String TransformIntoHTML(String markdown)
        {
            return _markdown.Transform(markdown);
        }
    }
}