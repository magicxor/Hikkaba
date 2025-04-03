using System.Collections.Generic;
using Hikkaba.Web.Services.Implementations;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public sealed class SimplifiedUrlRegexTests
{
    private static IEnumerable<TestCaseData> UriTestCases()
    {
        // Real-world examples
        yield return new TestCaseData("https://www.google.com/url?sa=t&source=web&rct=j&opi=89978449&url=https://www.am800cklw.com/news/new-us-tariffs-on-auto-sector-to-devastate-industry-raise-consumer-costs-leaders.html&ved=2ahUKEwiboKnt9aqMAxXcAjQIHTOlGkUQFnoECC8QAQ&usg=AOvVaw0DKJJURfq8WL58loZbS-BC", true);
        yield return new TestCaseData("https://www.amazon.xyz/bigspringsale/?_encoding=UTF8&pd_rd_w=BT9YD&content-id=amzn1.sym.3e529da7-0fed-4b58-963f-42ff3b3b010a&pf_rd_p=3e529da7-0fed-4b58-963f-42ff3b3b010a&pf_rd_r=XB171MA20QDR49RJZVK4&pd_rd_wg=G4rFP&pd_rd_r=9741a016-9f3c-4136-9c8a-85538f5e86cf&ref_=pd_hp_d_hero_unk", true);
        yield return new TestCaseData("https://www.amazon.xyz/MCIRCO-Oval-Glass-Casseroles-Lids/dp/B0CCY45M5T/?_encoding=UTF8&pd_rd_w=4mJvy&content-id=amzn1.sym.01ebc272-139a-4b73-af36-c8bb66d05c48%3Aamzn1.symc.bc83e277-f1bc-44bf-8b94-3140150ee013&pf_rd_p=01ebc272-139a-4b73-af36-c8bb66d05c48&pf_rd_r=XB171MA20QDR49RJZVK4&pd_rd_wg=g9NgT&pd_rd_r=a59c8a4d-31e1-4da2-af16-3f91e2d4b4bf&ref_=pd_hp_d_btf_ci_mcx_mr_hp_d_hf", true);
        yield return new TestCaseData("https://github.com/search?q=%3Ca%3E%5Blink%5D&type=repositories", true);

        // Internationalized Domain Names (IDNs)
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф", true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф/", true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф/#anchor-1", true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф/test", true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф:8080", true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф:8080/", true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф:8080/test", true);

        yield return new TestCaseData("http://www.人民网.中国", true);
        yield return new TestCaseData("http://www.人民网.中国/", true);
        yield return new TestCaseData("http://www.人民网.中国/#anchor-1", true);
        yield return new TestCaseData("http://www.人民网.中国/search?q=%3Ca%3E%5Blink%5D&type=repositories", true);
        yield return new TestCaseData("http://www.人民网.中国/search?q=%E5%AD%A6%5B", true);
        yield return new TestCaseData("http://www.人民网.中国:7852", true);
        yield return new TestCaseData("http://www.人民网.中国:7852/", true);
        yield return new TestCaseData("http://www.人民网.中国:7852/search?q=%3Ca%3E%5Blink%5D&type=repositories", true);
        yield return new TestCaseData("http://www.人民网.中国:7852/search?q=%E5%AD%A6%5B", true);

        yield return new TestCaseData("https://الصحة.السعودية", true);
        yield return new TestCaseData("https://الصحة.السعودية/", true);
        yield return new TestCaseData("https://الصحة.السعودية/#anchor-1", true);
        yield return new TestCaseData("https://الصحة.السعودية/search?q=%3Ca%3E%5Blink%5D&type=repositories", true);
        yield return new TestCaseData("https://الصحة.السعودية/search?q=%E5%AD%A6%5B", true);
        yield return new TestCaseData("https://الصحة.السعودية:8080", true);
        yield return new TestCaseData("https://الصحة.السعودية:8080/", true);
        yield return new TestCaseData("https://الصحة.السعودية:8080/search?q=%3Ca%3E%5Blink%5D&type=repositories", true);
        yield return new TestCaseData("https://الصحة.السعودية:8080/search?q=%E5%AD%A6%5B", true);

        // Punycode
        yield return new TestCaseData("https://xn-------43dababre1b5ahufa0ao8a9aoch5pugi.xn--p1ai:8080/", true);
        yield return new TestCaseData("https://xn-------43dababre1b5ahufa0ao8a9aoch5pugi.xn--p1ai/", true);

        // Usual URLs
        yield return new TestCaseData("https://google.com", true);
        yield return new TestCaseData("https://google.com/", true);
        yield return new TestCaseData("https://google.com/#anchor-1", true);
        yield return new TestCaseData("https://google.com/search?q=%3Ca%3E%5Blink%5D&type=repositories", true);
        yield return new TestCaseData("https://google.com/search?q=%E5%AD%A6%5B", true);
        yield return new TestCaseData("https://google.com/search?q=%3Ca%3E%5Blink%5D&type=repositories#anchor-1", true);

        // IPv4 without port
        yield return new TestCaseData("http://127.0.0.1", true);
        yield return new TestCaseData("http://127.0.0.1/", true);
        yield return new TestCaseData("http://127.0.0.1/search?q=%3Ca%3E%5Blink%5D&type=repositories", true);
        yield return new TestCaseData("http://127.0.0.1/search?q=%E5%AD%A6%5B", true);
        yield return new TestCaseData("http://127.0.0.1/search?q=%3Ca%3E%5Blink%5D&type=repositories#anchor-1", true);

        // IPv4 with port
        yield return new TestCaseData("http://127.0.0.1:7852", true);
        yield return new TestCaseData("http://127.0.0.1:7852/", true);
        yield return new TestCaseData("http://127.0.0.1:7852/search?q=%3Ca%3E%5Blink%5D&type=repositories", true);
        yield return new TestCaseData("http://127.0.0.1:7852/search?q=%E5%AD%A6%5B", true);
        yield return new TestCaseData("http://127.0.0.1:7852/search?q=%3Ca%3E%5Blink%5D&type=repositories#anchor-1", true);

        // Tricky cases (bug https://github.com/dotnet/runtime/issues/21626 / https://github.com/dotnet/runtime/issues/72632)
        yield return new TestCaseData("http://g.c/j?a=%2C", true);
        yield return new TestCaseData("http://g.c/j?a=%C3%A9", true);
        yield return new TestCaseData("http://g.c/j?a=%5B", true);
        yield return new TestCaseData("http://g.c/j?a=%5B#anchor-1", true);
        yield return new TestCaseData("http://g.c/j?a=%E5%AD%A6", true);
        yield return new TestCaseData("http://g.c/j?a=%2C%C3%A9", true);
        yield return new TestCaseData("http://g.c/j?a=%E5%AD%A6%5B", true);
        yield return new TestCaseData("https://www.bing.com/search?q=%D1%82%D0%B5%D1%81%D1%82%D0%BE%D0%B2%D1%8B%D0%B9%20%D0%B7%D0%B0%D0%BF%D1%80%D0%BE%D1%81%20123&qs=n&form=QBRE&sp=-1&ghc=1&lq=0&pq=%D1%82%D0%B5%D1%81%D1%82%D0%BE%D0%B2%D1%8B%D0%B9%20%D0%B7%D0%B0%D0%BF%D1%80%D0%BE%D1%81%20123&sc=4-19&sk=&cvid=DD300ABB1CAA454CB56B35C7F4EC47BF&ghsh=0&ghacc=0&ghpl=", true);

        // Invalid URLs
        yield return new TestCaseData("https://github.com/search?q=<a>&type=repositories", false).SetDescription("Invalid URL with unescaped '<' and '>' characters");
        yield return new TestCaseData("https://github.com/search?q=blah[link]blah&type=repositories", false).SetDescription("Invalid URL with unescaped '[' and ']' characters");
        yield return new TestCaseData("https://github.com/search?q=blah\"test\"blah&type=repositories", false).SetDescription("Invalid URL with unescaped double quotes");
        yield return new TestCaseData("https://github.com/search?q=кошка&type=repositories", false).SetDescription("Invalid URL with unescaped non-ASCII characters");
    }

    [TestCaseSource(nameof(UriTestCases))]
    public void Regex_ShouldMatchOnlyValidUrls(string input, bool wholeInputMatchesRegex)
    {
        var simplifiedUrlRegex = MessagePostProcessor.GetUriRegex();
        var match = simplifiedUrlRegex.Match(input);
        if (match.Success)
        {
            // Check if the entire input matches the regex
            Assert.That(match.Value, wholeInputMatchesRegex ? Is.EqualTo(input) : Is.Not.EqualTo(input));
        }
        else
        {
            // If the match is not successful, we check if the whole input should not match the regex
            Assert.That(wholeInputMatchesRegex, Is.False);
        }
    }
}
