using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;

namespace CodeKicker.BBCode.SyntaxTree
{
    public sealed class TextNode : SyntaxTreeNode
    {
        private readonly HtmlEncoder _htmlEncoder = HtmlEncoder.Default;

        public TextNode(string text)
            : this(text, null)
        {
        }
        public TextNode(string text, string htmlTemplate)
            : base(null)
        {
            if (text == null) throw new ArgumentNullException("text");
            Text = text;
            HtmlTemplate = htmlTemplate;
        }

        public string Text { get; private set; }
        public string HtmlTemplate { get; private set; }

        public override string ToHtml()
        {
            // Right-oh, future Paul - the below doesn't do anything sensible with newlines but the obvious 'replace newlines with BRs' tactic
            // doesn't work because you don't want to do it in all circumstances. Need to add a 'suppress BR after' property or similar to 
            // BBTag? This'd control if we trim the first newline after a tag of that type closes. We could *then* replace all \n with <br /> and
            // be on our merry way
            return (HtmlTemplate == null ? _htmlEncoder.Encode(Text) : HtmlTemplate.Replace("${content}", _htmlEncoder.Encode(Text))).Replace("\n", "<br />");
        }
        public override string ToBBCode()
        {
            return Text.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]");
        }
        public override string ToText()
        {
            return Text;
        }

        public override SyntaxTreeNode SetSubNodes(IEnumerable<SyntaxTreeNode> subNodes)
        {
            if (subNodes == null) throw new ArgumentNullException("subNodes");
            if (subNodes.Any()) throw new ArgumentException("subNodes cannot contain any nodes for a TextNode");
            return this;
        }
        internal override SyntaxTreeNode AcceptVisitor(SyntaxTreeVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException("visitor");
            return visitor.Visit(this);
        }

        protected override bool EqualsCore(SyntaxTreeNode b)
        {
            var casted = (TextNode)b;
            return Text == casted.Text && HtmlTemplate == casted.HtmlTemplate;
        }
    }
}