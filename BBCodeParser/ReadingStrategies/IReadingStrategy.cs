namespace BBCodeParser.ReadingStrategies;

public interface IReadingStrategy
{
    TagResult Read(string input);
}
