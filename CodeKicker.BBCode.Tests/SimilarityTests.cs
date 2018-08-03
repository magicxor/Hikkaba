using System.IO;
using System.Reflection;
using CodeKicker.BBCode;
using Xunit;

namespace Narochno.BBCode.Tests
{
    public class SimilarityTests
    {
        internal string GetResource(string name)
        {
            Assembly assembly = typeof(SimilarityTests).GetTypeInfo().Assembly;
            string qualifiedName = $"{assembly.GetName().Name}.Files.{name}";
            using (var sr = new StreamReader(assembly.GetManifestResourceStream(qualifiedName)))
            {
                return sr.ReadToEnd();
            }
        }

        [Theory]
        [InlineData("test1.bb", "test1.html")]
        [InlineData("test2.bb", "test2.html")]
        public void TestParseDocuments(string inputFile, string outputFile)
        {
            var input = File.ReadAllText(Path.Combine("Files", inputFile));
            var parser = new BBCodeParser(ErrorMode.ErrorFree, null, new[]
            {
                new BBTag("b", "<b>", "</b>"),
                new BBTag("i", "<em>", "</em>"),
                new BBTag("u", "<u>", "</u>"),
                new BBTag("h1", "<h1>", "</h1>"),
                new BBTag("code", "<pre>", "</pre>"),
                new BBTag("img", "<img src=\"${content}\" />", "", false, true),
                new BBTag("quote", "<blockquote>", "</blockquote>"),
                new BBTag("list", "<ul>", "</ul>"),
                new BBTag("*", "<li>", "</li>", true, false),
                new BBTag("url", "<a href=\"${href}\">", "</a>", new BBAttribute("href", ""), new BBAttribute("href", "href")),
            });
            var actualOutput = parser.ToHtml(input);
            var expectedOutput = File.ReadAllText(Path.Combine("Files", outputFile));

            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
