using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace CodeKicker.BbCode.Tests.Unit;

public class SimilarityTests
{
    internal string GetResource(string name)
    {
        Assembly assembly = typeof(SimilarityTests).GetTypeInfo().Assembly;
        string qualifiedName = $"{assembly.GetName().Name}.Files.{name}";
        var stream = assembly.GetManifestResourceStream(qualifiedName) ?? throw new UnitTestConfigException($"Resource {qualifiedName} not found.");
        using var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }

    [Theory]
    [InlineData("test1.bb", "test1.html")]
    [InlineData("test2.bb", "test2.html")]
    public void TestParseDocuments(string inputFile, string outputFile)
    {
        var input = File.ReadAllText(Path.Combine("Files", inputFile));
        var parser = new BbCodeParser(ErrorMode.ErrorFree, null, [
            new BbTag("b", "<b>", "</b>"),
            new BbTag("i", "<em>", "</em>"),
            new BbTag("u", "<u>", "</u>"),
            new BbTag("h1", "<h1>", "</h1>"),
            new BbTag("code", "<pre>", "</pre>"),
            new BbTag("img", "<img src=\"${content}\" />", "", false, true),
            new BbTag("quote", "<blockquote>", "</blockquote>"),
            new BbTag("list", "<ul>", "</ul>"),
            new BbTag("*", "<li>", "</li>", true, false),
            new BbTag("url", "<a href=\"${href}\">", "</a>", new BbAttribute("href", ""), new BbAttribute("href", "href")),
        ]);
        var actualOutput = parser.ToHtml(input);
        var expectedOutput = File.ReadAllText(Path.Combine("Files", outputFile));

        Assert.Equal(expectedOutput, actualOutput);
    }
}
