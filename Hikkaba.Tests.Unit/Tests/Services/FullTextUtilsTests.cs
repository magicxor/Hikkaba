using Hikkaba.Infrastructure.Repositories.Utils;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class FullTextUtilsTests
{
    [TestCase("a", "\"a\"")]
    [TestCase("abc", "\"abc\"")]
    [TestCase("\"abc\"", "\"abc\"")]
    [TestCase("\"\"abc\"", "\"abc\"")]
    [TestCase("\"\"\"abc\"", "\"abc\"")]
    [TestCase("\"\"abc\"\"\"", "\"abc\"")]
    [TestCase("hello world", "\"hello\" AND \"world\"")]
    [TestCase("hello  world", "\"hello\" AND \"world\"")]
    [TestCase("hello   world", "\"hello\" AND \"world\"")]
    [TestCase("\"hello\" \"world\"", "\"hello\" AND \"world\"")]
    [TestCase("hello \"  world", "\"hello\" AND \"world\"")]
    [TestCase("hello \" \"  world", "\"hello\" AND \"world\"")]
    [TestCase("""
              hello
                world
              """, "\"hello\" AND \"world\"")]
    [TestCase("""
              hello
               "
                world
              """, "\"hello\" AND \"world\"")]
    [TestCase("""
              "hello
               "
                "world"
              """, "\"hello\" AND \"world\"")]
    [TestCase("FORMSOF(INFLECTIONAL, cat)", "\"FORMSOF(INFLECTIONAL,\" AND \"cat)\"")]
    [TestCase("NEAR((dog, cat),5,TRUE)", "\"NEAR((dog,\" AND \"cat),5,TRUE)\"")]
    [TestCase("ISABOUT((apple weight(0.8), banana weight(0.2)))", "\"ISABOUT((apple\" AND \"weight(0.8),\" AND \"banana\" AND \"weight(0.2)))\"")]
    [TestCase("dog AND cat", "\"dog\" AND \"AND\" AND \"cat\"")]
    [TestCase("dog OR cat", "\"dog\" AND \"OR\" AND \"cat\"")]
    [TestCase("dog NOT cat", "\"dog\" AND \"NOT\" AND \"cat\"")]
    [TestCase("dog NEAR cat", "\"dog\" AND \"NEAR\" AND \"cat\"")]
    [TestCase("dog|cat", "\"dog|cat\"")]
    [TestCase("wild*", "\"wild\"")]
    [TestCase("\"wild*\"", "\"wild\"")]
    [TestCase("wild* abc*", "\"wild\" AND \"abc\"")]
    [TestCase("*wild", "\"wild\"")]
    [TestCase("\"*wild\"", "\"wild\"")]
    [TestCase("*wild *abc", "\"wild\" AND \"abc\"")]
    [TestCase("*wild*", "\"wild\"")]
    [TestCase("\"*wild*\"", "\"wild\"")]
    [TestCase("*wild* *abc*", "\"wild\" AND \"abc\"")]
    [TestCase("\"*wi*ld*\"", "\"wi\" AND \"ld\"")]
    [TestCase("\"*wi *ld*\"", "\"wi\" AND \"ld\"")]
    [TestCase("\"*wi * ld*\"", "\"wi\" AND \"ld\"")]
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("*", "")]
    [TestCase(@"\*", @"""\""")]
    [TestCase("\"*", "")]
    [TestCase("\" * \" * ", "")]
    [TestCase(" * ", "")]
    [TestCase("apostrophes' ", "\"apostrophes'\"")]
    public void GetFullText_ShouldReturnCorrectFullText(string? text, string expected)
    {
        var result = FullTextUtils.ConvertToSearchableString(text);
        Assert.That(result, Is.EqualTo(expected));
    }
}
