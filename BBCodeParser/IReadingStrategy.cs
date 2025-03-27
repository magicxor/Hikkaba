namespace BBCodeParser;

public interface IReadingStrategy
{
    TagResult Read(string input);
}