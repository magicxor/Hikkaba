using System;
using System.Collections.Generic;
using BBCodeParser;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public class UrlUtilityTests
{
    private static IEnumerable<TestCaseData> UriTestCases()
    {
        // Real-world examples
        yield return new TestCaseData("https://www.google.com/url?sa=t&source=web&rct=j&opi=89978449&url=https://www.am800cklw.com/news/new-us-tariffs-on-auto-sector-to-devastate-industry-raise-consumer-costs-leaders.html&ved=2ahUKEwiboKnt9aqMAxXcAjQIHTOlGkUQFnoECC8QAQ&usg=AOvVaw0DKJJURfq8WL58loZbS-BC", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://www.amazon.xyz/bigspringsale/?_encoding=UTF8&pd_rd_w=BT9YD&content-id=amzn1.sym.3e529da7-0fed-4b58-963f-42ff3b3b010a&pf_rd_p=3e529da7-0fed-4b58-963f-42ff3b3b010a&pf_rd_r=XB171MA20QDR49RJZVK4&pd_rd_wg=G4rFP&pd_rd_r=9741a016-9f3c-4136-9c8a-85538f5e86cf&ref_=pd_hp_d_hero_unk", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://www.amazon.xyz/MCIRCO-Oval-Glass-Casseroles-Lids/dp/B0CCY45M5T/?_encoding=UTF8&pd_rd_w=4mJvy&content-id=amzn1.sym.01ebc272-139a-4b73-af36-c8bb66d05c48%3Aamzn1.symc.bc83e277-f1bc-44bf-8b94-3140150ee013&pf_rd_p=01ebc272-139a-4b73-af36-c8bb66d05c48&pf_rd_r=XB171MA20QDR49RJZVK4&pd_rd_wg=g9NgT&pd_rd_r=a59c8a4d-31e1-4da2-af16-3f91e2d4b4bf&ref_=pd_hp_d_btf_ci_mcx_mr_hp_d_hf", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://github.com/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);

        // Internationalized Domain Names (IDNs)
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф/test", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф:8080", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф:8080/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://самая-красивая-кошка-в-мире.рф:8080/test", UriKind.Absolute).Returns(true);

        yield return new TestCaseData("http://www.人民网.中国", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://www.人民网.中国/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://www.人民网.中国/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://www.人民网.中国/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://www.人民网.中国:7852", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://www.人民网.中国:7852/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://www.人民网.中国:7852/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://www.人民网.中国:7852/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        yield return new TestCaseData("https://الصحة.السعودية", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://الصحة.السعودية/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://الصحة.السعودية/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://الصحة.السعودية/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://الصحة.السعودية:8080", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://الصحة.السعودية:8080/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://الصحة.السعودية:8080/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://الصحة.السعودية:8080/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        // Punycode
        yield return new TestCaseData("https://xn-------43dababre1b5ahufa0ao8a9aoch5pugi.xn--p1ai:8080/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://xn-------43dababre1b5ahufa0ao8a9aoch5pugi.xn--p1ai/", UriKind.Absolute).Returns(true);

        // Usual URLs
        yield return new TestCaseData("https://google.com", UriKind.Relative).Returns(false);
        yield return new TestCaseData("https://google.com", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://google.com/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://google.com/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://google.com/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        // IPv4 without port
        yield return new TestCaseData("http://127.0.0.1", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://127.0.0.1/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://127.0.0.1/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://127.0.0.1/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        // IPv4 with port
        yield return new TestCaseData("http://127.0.0.1:7852", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://127.0.0.1:7852/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://127.0.0.1:7852/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://127.0.0.1:7852/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        // IPv6 without port
        yield return new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        // IPv6 with port
        yield return new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]:7852", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]:7852/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]:7852/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[1080:0:0:0:8:800:200C:417A]:7852/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        // IPv6 loopback without port
        yield return new TestCaseData("http://[::1]", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[::1]/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[::1]/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[::1]/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        // IPv6 loopback with port
        yield return new TestCaseData("http://[::1]:7852", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[::1]:7852/", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[::1]:7852/search?q=%3Ca%3E%5Blink%5D&type=repositories", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://[::1]:7852/search?q=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);

        // Tricky cases (bug https://github.com/dotnet/runtime/issues/21626 / https://github.com/dotnet/runtime/issues/72632)
        yield return new TestCaseData("http://g.c/j?a=%2C", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://g.c/j?a=%C3%A9", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://g.c/j?a=%5B", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://g.c/j?a=%E5%AD%A6", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://g.c/j?a=%2C%C3%A9", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("http://g.c/j?a=%E5%AD%A6%5B", UriKind.Absolute).Returns(true);
        yield return new TestCaseData("https://www.bing.com/search?q=%D1%82%D0%B5%D1%81%D1%82%D0%BE%D0%B2%D1%8B%D0%B9%20%D0%B7%D0%B0%D0%BF%D1%80%D0%BE%D1%81%20123&qs=n&form=QBRE&sp=-1&ghc=1&lq=0&pq=%D1%82%D0%B5%D1%81%D1%82%D0%BE%D0%B2%D1%8B%D0%B9%20%D0%B7%D0%B0%D0%BF%D1%80%D0%BE%D1%81%20123&sc=4-19&sk=&cvid=DD300ABB1CAA454CB56B35C7F4EC47BF&ghsh=0&ghacc=0&ghpl=", UriKind.Absolute).Returns(true);

        // Invalid URLs
        yield return new TestCaseData("https://github.com/search?q=<a>&type=repositories", UriKind.Absolute).Returns(false).SetDescription("Invalid URL with unescaped '<' and '>' characters");
        yield return new TestCaseData("https://github.com/search?q=blah[link]blah&type=repositories", UriKind.Absolute).Returns(false).SetDescription("Invalid URL with unescaped '[' and ']' characters");
        yield return new TestCaseData("https://github.com/search?q=blah\"test\"blah&type=repositories", UriKind.Absolute).Returns(false).SetDescription("Invalid URL with unescaped double quotes");
        yield return new TestCaseData("https://github.com/search?q=кошка&type=repositories", UriKind.Absolute).Returns(false).SetDescription("Invalid URL with unescaped non-ASCII characters");
        yield return new TestCaseData("/a", UriKind.Absolute).Returns(false).SetDescription("Relative path with absolute URI kind");
        yield return new TestCaseData("/j?a=%5B", UriKind.Relative).Returns(true);
        yield return new TestCaseData("/j?a=%E5%AD%A6", UriKind.Relative).Returns(true);
        yield return new TestCaseData("/j?a=%2C%C3%A9", UriKind.Relative).Returns(true);
        yield return new TestCaseData("/j?a=%E5%AD%A6%5B", UriKind.Relative).Returns(true);
        yield return new TestCaseData("/search?q=<a>&type=repositories", UriKind.Relative).Returns(false).SetDescription("Invalid URL with unescaped '<' and '>' characters");
        yield return new TestCaseData("/search?q=blah[link]blah&type=repositories", UriKind.Relative).Returns(false).SetDescription("Invalid URL with unescaped '[' and ']' characters");
        yield return new TestCaseData("/search?q=blah\"test\"blah&type=repositories", UriKind.Relative).Returns(false).SetDescription("Invalid URL with unescaped double quotes");
        yield return new TestCaseData("/search?q=кошка&type=repositories", UriKind.Relative).Returns(false).SetDescription("Invalid URL with unescaped non-ASCII characters");

        // Edge cases
        yield return new TestCaseData("", UriKind.Relative).Returns(false).SetDescription("Empty string - invalid");
        yield return new TestCaseData("", UriKind.Absolute).Returns(false).SetDescription("Empty string - invalid");
        yield return new TestCaseData("/", UriKind.Relative).Returns(true).SetDescription("Relative URI: root path '/'");
        yield return new TestCaseData("/", UriKind.Absolute).Returns(false).SetDescription("Relative URI with absolute URI kind");
        yield return new TestCaseData("?q=test", UriKind.Relative).Returns(false).SetDescription("Relative URI: query string only - controversial edge case, not sure if valid [TODO]");
        yield return new TestCaseData("?q=test", UriKind.Absolute).Returns(false).SetDescription("Relative URI with absolute URI kind");
        yield return new TestCaseData("#anchor", UriKind.Relative).Returns(true).SetDescription("Relative URI: fragment only");
        yield return new TestCaseData("#a", UriKind.Relative).Returns(true).SetDescription("Relative URI: fragment only");
        yield return new TestCaseData("#anchor", UriKind.Absolute).Returns(false).SetDescription("Relative URI with absolute URI kind");
        yield return new TestCaseData("//example.com/path", UriKind.Relative).Returns(true).SetDescription("Relative URI: authority and path without a scheme");
        yield return new TestCaseData("http:", UriKind.Absolute).Returns(false).SetDescription("Scheme only, no slashes or domain");
        yield return new TestCaseData("http://", UriKind.Absolute).Returns(false).SetDescription("Scheme plus slashes, but no domain or path");
        yield return new TestCaseData("http://", UriKind.Relative).Returns(false).SetDescription("Scheme plus slashes, but no domain or path");
        yield return new TestCaseData("example.com", UriKind.Absolute).Returns(false).SetDescription("Domain only, no scheme");
        yield return new TestCaseData("example.com/a/b/c?g=1&p=3", UriKind.Absolute).Returns(false).SetDescription("Domain only, no scheme");
        yield return new TestCaseData(":8080", UriKind.Absolute).Returns(false).SetDescription("Port only, no scheme or domain");
        yield return new TestCaseData(":8080", UriKind.Relative).Returns(false).SetDescription("Port only, no scheme or domain");
        yield return new TestCaseData("ftp://ftp.example.com/pub/file.txt", UriKind.Absolute).Returns(true).SetDescription("FTP scheme");

        // User and password
        yield return new TestCaseData("http://user@example.com", UriKind.Absolute).Returns(true).SetDescription("User info without password");
        yield return new TestCaseData("http://user:password@example.com", UriKind.Absolute).Returns(true).SetDescription("User and password");
        yield return new TestCaseData("http://user:password@example.com:8080/test/path", UriKind.Absolute).Returns(true).SetDescription("User info, domain, port, path");
        yield return new TestCaseData("mailto:user@example.com", UriKind.Absolute).Returns(true).SetDescription("Mailto scheme");
        yield return new TestCaseData("http://example.com:abc", UriKind.Absolute).Returns(false).SetDescription("Invalid port format");
        yield return new TestCaseData("https://user@example.com", UriKind.Absolute).Returns(true).SetDescription("URI with userinfo (username only; no password)");
        yield return new TestCaseData("https://user:pass@example.com", UriKind.Absolute).Returns(true).SetDescription("URI with userinfo (username and password)");
        yield return new TestCaseData("https://user:@example.com", UriKind.Absolute).Returns(true).SetDescription("URI with userinfo (empty password after colon)");
        yield return new TestCaseData("https://user:pass@example.com:8080/path?query=val#frag", UriKind.Absolute).Returns(true).SetDescription("Full URI with all components (userinfo; host; port; path; query; fragment)");
        yield return new TestCaseData("ftp://user:pass@192.168.0.1:2121/dir/file.txt", UriKind.Absolute).Returns(true).SetDescription("URI with userinfo on an IPv4 address and a non-standard port");
        yield return new TestCaseData("sftp://user:pass@sftp.example.com/home", UriKind.Absolute).Returns(true).SetDescription("URI with the sftp scheme and userinfo");
        yield return new TestCaseData("http://us er:pass@example.com", UriKind.Absolute).Returns(false).SetDescription("Space in username is invalid");
        yield return new TestCaseData("http://user:pa ss@example.com", UriKind.Absolute).Returns(false).SetDescription("Space in password is invalid");
    }

    [TestCaseSource(nameof(UriTestCases))]
    public bool IsWellFormedUriString_WhenCalled_ShouldReturnCorrectResult(string uriStr, UriKind uriKind)
    {
        return UriUtility.IsWellFormedUriString(uriStr, uriKind);
    }
}
