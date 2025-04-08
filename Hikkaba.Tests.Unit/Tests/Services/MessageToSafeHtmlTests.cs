using Hikkaba.Web.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class MessageToSafeHtmlTests
{
    private const string FakeActionPath = "/b/23454362";

    [TestCase("ＳＯＭＥ　ＳＴＲＡＮＧＥ　ＴＥＸＴ，　ｈｕｈ　^^　延凹線艶彙")]
    [TestCase("Ｓ♢ＭΞ░ＳＴＲΛＮＧΞ░ＴΞＸＴ，░ｈｕｈ░^^　（延凹線艶彙）")]
    [TestCase("【﻿ＳＯＭＥ　ＳＴＲＡＮＧＥ　ＴＥＸＴ，　ｈｕｈ　^^】")]
    [TestCase("丂ㄖ爪乇　丂ㄒ尺卂几Ꮆ乇　ㄒ乇乂ㄒ，　卄ㄩ卄　^^")]
    [TestCase("己回冊ヨ　己卞尺丹几呂ヨ　卞ヨメ卞，　廾凵廾　^^")]
    [TestCase("丂のﾶ乇　丂ｲ尺ﾑ刀ム乇　ｲ乇ﾒｲ，　んひん　^^")]
    [TestCase("Some letters. 1234567890; 987 * 2 - 5 @! | [wow](!wow)[!wow][[ yoy )))) [[[ ]] ] \\ //.")]
    public void MessageToSafeHtml_WhenCalledWithText_ShouldReturnTheSameText(string input)
    {
        using var customAppFactory = new CustomAppFactory(FakeActionPath);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToSafeHtml("a", 3453453434, input);
        Assert.That(actualOutput, Is.EqualTo(input));
    }

    [TestCase("TEXT\r\nWITH\r\nLINE\r\nBREAKS", "TEXT\nWITH\nLINE\nBREAKS")]
    [TestCase("TEXT\r\n\r\n\r\nWITH LINE BREAKS", "TEXT\n\nWITH LINE BREAKS")]
    [TestCase("TEXT\r\nWITH LINE BREAKS", "TEXT\nWITH LINE BREAKS")]
    [TestCase("TEXT\nWITH LINE BREAKS", "TEXT\nWITH LINE BREAKS")]
    [TestCase("TEXT\n\nWITH LINE BREAKS", "TEXT\n\nWITH LINE BREAKS")]
    [TestCase("TEXT\n\rWITH LINE BREAKS", "TEXT\n\nWITH LINE BREAKS")]
    public void MessageToSafeHtml_WhenCalledWithLineBreaks_ShouldReturnNormalizedLineBreaks(string input, string expectedOutput)
    {
        using var customAppFactory = new CustomAppFactory(FakeActionPath);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToSafeHtml("a", 43254325, input);
        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }

    [TestCase("[b]bold[/b]", "<b>bold</b>")]
    [TestCase("[i]italic[/i]", "<i>italic</i>")]
    [TestCase("[u]underline[/u]", "<u>underline</u>")]
    [TestCase("[s]strikethrough[/s]", "<s>strikethrough</s>")]
    [TestCase("[code]preformatted[/code]", "<pre class=\"code\" data-syntax=\"\">preformatted</pre>")]
    [TestCase("[sub]subscript[/sub]", "<sub>subscript</sub>")]
    [TestCase("[sup]superscript[/sup]", "<sup>superscript</sup>")]
    [TestCase("[spoiler]spoiler[/spoiler]", """<span class="censored">spoiler</span>""")]
    [TestCase("[quote]quote[/quote]", """<span class="text-success">&gt; quote</span>""")]
    [TestCase("plain [b][i][u]mix3[/u] mix2[/i] mix1[/b] plain", "plain <b><i><u>mix3</u> mix2</i> mix1</b> plain")]
    public void MessageToSafeHtml_WhenCalledWithBbCode_ShouldReturnHtml(string input, string expectedOutput)
    {
        using var customAppFactory = new CustomAppFactory(FakeActionPath);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToSafeHtml("a", 345345345, input);
        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }

    [TestCase("http://example.com", """<a href="http://example.com" rel="nofollow noopener noreferrer external">http://example.com</a>""")]
    [TestCase("https://example.com", """<a href="https://example.com" rel="nofollow noopener noreferrer external">https://example.com</a>""")]
    [TestCase("ftp://example.com", """<a href="ftp://example.com" rel="nofollow noopener noreferrer external">ftp://example.com</a>""")]
    [TestCase("http://example.com/item/a-b-c/1823888278.html?spm=2114.30010708.3.17.2rt7qZ&ws_ab_test=searchweb201556_8,searchweb201602_", """<a href="http://example.com/item/a-b-c/1823888278.html?spm=2114.30010708.3.17.2rt7qZ&ws_ab_test=searchweb201556_8,searchweb201602_" rel="nofollow noopener noreferrer external">http://example.com/item/a-b-c/1823888278.html?spm=2114.30010708.3.17.2rt7qZ&amp;ws_ab_test=searchweb201556_8,searchweb201602_</a>""")]
    [TestCase("https://github.com/search?q=+created%3A%3E2020-03-23+test&type=repositories", """<a href="https://github.com/search?q=+created%3A%3E2020-03-23+test&type=repositories" rel="nofollow noopener noreferrer external">https://github.com/search?q=+created%3A%3E2020-03-23+test&amp;type=repositories</a>""")]
    [TestCase("https://github.com/search?q=repo%3Atest%2FTest+encode%5D%5D&type=code", """<a href="https://github.com/search?q=repo%3Atest%2FTest+encode%5D%5D&type=code" rel="nofollow noopener noreferrer external">https://github.com/search?q=repo%3Atest%2FTest+encode%5D%5D&amp;type=code</a>""")]
    /* invalid characters next to the link: */
    [TestCase("http://example.com/[b]bold[/b]", """<a href="http://example.com/" rel="nofollow noopener noreferrer external">http://example.com/</a><b>bold</b>""")]
    [TestCase("http://example.com/<b>bold</b>", """<a href="http://example.com/" rel="nofollow noopener noreferrer external">http://example.com/</a>&lt;b&gt;bold&lt;/b&gt;""")]
    [TestCase("http://example.com/\"bold\"abc", """<a href="http://example.com/" rel="nofollow noopener noreferrer external">http://example.com/</a>"bold"abc""")]
    public void MessageToSafeHtml_WhenCalledWithLinks_ShouldReturnHtmlLinks(string input, string expectedOutput)
    {
        using var customAppFactory = new CustomAppFactory(FakeActionPath);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToSafeHtml("a", 12387, input);
        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }

    [TestCase(">>0", $"""<a href="{FakeActionPath}#0">&gt;&gt;0</a>""")]
    [TestCase(">>999", $"""<a href="{FakeActionPath}#999">&gt;&gt;999</a>""")]
    public void MessageToSafeHtml_WhenCalledWithReplyLinks_ShouldReturnHtmlLinks(string input, string expectedOutput)
    {
        using var customAppFactory = new CustomAppFactory(FakeActionPath);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToSafeHtml("a", 9024389, input);
        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }

    [TestCase("<script>alert('qq')</script>", "&lt;script&gt;alert('qq')&lt;/script&gt;")]
    [TestCase("<div>test</div>", "&lt;div&gt;test&lt;/div&gt;")]
    [TestCase("<article>test</article>", "&lt;article&gt;test&lt;/article&gt;")]
    [TestCase("<nav>test</nav>", "&lt;nav&gt;test&lt;/nav&gt;")]
    public void MessageToSafeHtml_WhenCalledWithUnsafeTags_ShouldReturnHtml(string input, string expectedOutput)
    {
        using var customAppFactory = new CustomAppFactory(FakeActionPath);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToSafeHtml("a", 234092, input);
        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }

    // Test malicious input that tries to execute XSS.
    [TestCase("<SCRIPT SRC=http://evil.com/xss.js></SCRIPT>", """&lt;SCRIPT SRC=<a href="http://evil.com/xss.js" rel="nofollow noopener noreferrer external">http://evil.com/xss.js</a>&gt;&lt;/SCRIPT&gt;""")]
    [TestCase("<img src=x onerror=alert('XSS')>", "&lt;img src=x onerror=alert('XSS')&gt;")]
    [TestCase("<svg/onload=alert(1)>", "&lt;svg/onload=alert(1)&gt;")]
    [TestCase("\"><script>alert('XSS')</script>", "\"&gt;&lt;script&gt;alert('XSS')&lt;/script&gt;")]
    [TestCase("<IMG SRC=javascript:alert('XSS')>", "&lt;IMG SRC=javascript:alert('XSS')&gt;")]
    [TestCase("<<script>script>alert(\"XSS\")</<script>script>", "&lt;&lt;script&gt;script&gt;alert(\"XSS\")&lt;/&lt;script&gt;script&gt;")]
    [TestCase("<scr<script>ipt>alert('XSS')</scr<script>ipt>", "&lt;scr&lt;script&gt;ipt&gt;alert('XSS')&lt;/scr&lt;script&gt;ipt&gt;")]
    [TestCase("<a href=\"javascript:alert('XSS')\">Click me</a>", "&lt;a href=\"javascript:alert('XSS')\"&gt;Click me&lt;/a&gt;")]
    // Test injection through a combination of bb-code and HTML.
    [TestCase("[b]Bold[/b]<img src=x onerror=alert('XSS')>", "<b>Bold</b>&lt;img src=x onerror=alert('XSS')&gt;")]
    // Check that a string with already encoded HTML entities remains unchanged
    // todo: investigate why this could be a problem
    /* [TestCase("&#x3C;script&#x3E;alert('XSS')&#x3C;/script&#x3E;", "&#x3C;script&#x3E;alert('XSS')&#x3C;/script&#x3E;")] */
    // Check line break normalization along with injection
    [TestCase("Text\n<script>alert(1)</script>\nText", "Text\n&lt;script&gt;alert(1)&lt;/script&gt;\nText")]
    // If an attempt is made to use bb-code for a link with an unsuitable protocol
    [TestCase("[url]javascript:alert('XSS')[/url]", """<a href="" rel="nofollow noopener noreferrer external">javascript:alert('XSS')</a>""")]
    [TestCase("""[url="javascript:alert('XSS')"]test[/url]""", """<a href="_xss_alert('XSS')" rel="nofollow noopener noreferrer external">test</a>""")]
    [TestCase("[relurl]javascript:alert('XSS')[/relurl]", """<a href="">javascript:alert('XSS')</a>""")]
    [TestCase("""[relurl="javascript:alert('XSS')"]test[/relurl]""", """<a href="javascript%3aalert(%27XSS%27)">test</a>""")]
    // Attribute injection
    [TestCase("""[url="[url="javascript:alert('XSS')"]"]test[/url]""", """[url="<a href="_xss_alert('XSS')" rel="nofollow noopener noreferrer external">"]test</a>""")]
    [TestCase("""[url="[url="javascript:alert('XSS')"][/url]"]test[/url]""", """[url="<a href="_xss_alert('XSS')" rel="nofollow noopener noreferrer external"></a>"]test""")]
    public void MessageToSafeHtml_WhenCalledWithMaliciousInput_ShouldSanitizeInput(string input, string expectedOutput)
    {
        using var customAppFactory = new CustomAppFactory(FakeActionPath);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToSafeHtml("a", 987654321, input);
        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }

    [TestCase("[code]text[/code]", "<pre class=\"code\" data-syntax=\"\">text</pre>")]
    [TestCase("[code]text\ntext[/code]", "<pre class=\"code\" data-syntax=\"\">text\ntext</pre>")]
    [TestCase("[code]text\r\ntext[/code]", "<pre class=\"code\" data-syntax=\"\">text\ntext</pre>")]
    [TestCase("[code]<a>text</a>[/code]", "<pre class=\"code\" data-syntax=\"\">&lt;a&gt;text&lt;/a&gt;</pre>")]
    [TestCase("[code]<a href=\"javascript:alert('XSS')\">Click me</a>[/code]", "<pre class=\"code\" data-syntax=\"\">&lt;a href=\"javascript:alert('XSS')\"&gt;Click me&lt;/a&gt;</pre>")]
    [TestCase("[code]<script>alert('XSS')</script>[/code]", "<pre class=\"code\" data-syntax=\"\">&lt;script&gt;alert('XSS')&lt;/script&gt;</pre>")]
    [TestCase("[code]<img src=x onerror=alert('XSS')>[/code]", "<pre class=\"code\" data-syntax=\"\">&lt;img src=x onerror=alert('XSS')&gt;</pre>")]
    [TestCase("[code][b]bold[/b][/code]", "<pre class=\"code\" data-syntax=\"\">[b]bold[/b]</pre>")]
    [TestCase("[code][i]italic[/i][/code]", "<pre class=\"code\" data-syntax=\"\">[i]italic[/i]</pre>")]
    [TestCase("""[code="csharp"]text[/code]""", "<pre class=\"code\" data-syntax=\"csharp\">text</pre>")]
    [TestCase("""[code="javascript"]text[/code]""", "<pre class=\"code\" data-syntax=\"javascript\">text</pre>")]
    [TestCase("""[code="javascript:alert('XSS')"]text[/code]""", "<pre class=\"code\" data-syntax=\"_xss_alert(XSS)\">text</pre>")]
    public void MessageToSafeHtml_WhenCalledWithPreformattedTag_ShouldReturnHtml(string input, string expectedOutput)
    {
        using var customAppFactory = new CustomAppFactory(FakeActionPath);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToSafeHtml("a", 209483, input);
        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }
}
