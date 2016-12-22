using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Hikkaba.Service.MarkdigAddons.Parsers
{
    public class SingleQuoteBlockParser : BlockParser, IAttributesParseable
    {
        public SingleQuoteBlockParser()
        {
            OpeningCharacters = new[] { '>' };
        }

        public TryParseAttributesDelegate TryParseAttributes { get; set; }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            // If we are in a CodeIndent, early exit
            if (processor.IsCodeIndent)
            {
                return BlockState.None;
            }

            var column = processor.Column;
            var line = processor.Line;
            var sourcePosition = line.Start;
            var c = line.CurrentChar;
            var matchingChar = c;

            int leadingCount = 0;
            while (c != '\0' && leadingCount <= 6)
            {
                if (c != matchingChar)
                {
                    break;
                }
                c = line.NextChar();
                leadingCount++;
            }

            // A space is required after leading #
            if (leadingCount > 0 && leadingCount <= 6 && (c.IsSpaceOrTab() || c == '\0'))
            {
                // Move to the content
                var singleQuoteBlock = new MarkdigAddons.Blocks.SingleQuoteBlock(this)
                {
                    HeaderChar = matchingChar,
                    Level = leadingCount,
                    Column = column,
                    Span = { Start = sourcePosition }
                };
                processor.NewBlocks.Push(singleQuoteBlock);
                processor.GoToColumn(column + leadingCount + 1);
                
                // The optional closing sequence of #s must be preceded by a space and may be followed by spaces only.
                int endState = 0;
                int countClosingTags = 0;
                for (int i = processor.Line.End; i >= processor.Line.Start - 1; i--)  // Go up to Start - 1 in order to match the space after the first ###
                {
                    c = processor.Line.Text[i];
                    if (endState == 0)
                    {
                        if (c.IsSpaceOrTab())
                        {
                            continue;
                        }
                        endState = 1;
                    }
                    if (endState == 1)
                    {
                        if (c == matchingChar)
                        {
                            countClosingTags++;
                            continue;
                        }

                        if (countClosingTags > 0)
                        {
                            if (c.IsSpaceOrTab())
                            {
                                processor.Line.End = i - 1;
                            }
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                // Setup the source end position of this element
                singleQuoteBlock.Span.End = processor.Line.End;

                // We expect a single line, so don't continue
                return BlockState.Break;
            }
            
            return BlockState.None;
        }

        public override bool Close(BlockProcessor processor, Block block)
        {
            var singleQuoteBlock = (MarkdigAddons.Blocks.SingleQuoteBlock)block;
            singleQuoteBlock.Lines.Trim();
            return true;
        }
    }
}
