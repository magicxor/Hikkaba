using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Hikkaba.Service.Extensions.SingleQuoteBlock
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
