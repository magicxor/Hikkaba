using System.Diagnostics;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Hikkaba.Service.MarkdigAddons.Blocks
{
    [DebuggerDisplay("{GetType().Name} Line: {Line}, {Lines} Level: {Level}")]
    public class SingleQuoteBlock : LeafBlock
    {
        public SingleQuoteBlock(BlockParser parser) : base(parser)
        {
            ProcessInlines = true;
        }
        public char HeaderChar { get; set; }
        public int Level { get; set; }
    }
}
