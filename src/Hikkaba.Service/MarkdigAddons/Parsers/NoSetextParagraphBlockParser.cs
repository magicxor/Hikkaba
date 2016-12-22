using Markdig.Parsers;
using Markdig.Syntax;

namespace Hikkaba.Service.MarkdigAddons.Parsers
{
    public class NoSetextParagraphBlockParser: ParagraphBlockParser
    {
        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            if (processor.IsBlankLine)
            {
                return BlockState.BreakDiscard;
            }
            
            block.UpdateSpanEnd(processor.Line.End);
            return BlockState.Continue;
        }
    }
}
