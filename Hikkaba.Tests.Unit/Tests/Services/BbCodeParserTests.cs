using System;
using System.Collections.Generic;
using System.Linq;
using BBCodeParser;
using BBCodeParser.Nodes;
using BBCodeParser.Tags;
using NUnit.Framework.Legacy;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
public class BbCodeParserTests
{
    private BBParser _bbCodeParser = new([new Tag("b", "<strong>", "</strong>")], [], []);
    private const string CodeClassName = "code";
    private const string PreClassName = "pref";
    private const string SpoilerHeadClassName = "spoiler-head";
    private const string SpoilerClassName = "spoiler";
    private const string ImageClassName = "image";
    private const string QuoteClassName = "quote";
    private const string QuoteAuthorClassName = "quote-author";

    [SetUp]
    public void SetUp()
    {
        _bbCodeParser = new BBParser([
                new Tag("b", "<strong>", "</strong>"),
                new Tag("i", "<em>", "</em>"),
                new Tag("u", "<u>", "</u>"),
                new Tag("s", "<s>", "</s>"),
                new PreformattedTag("pre", $"<pre class=\"{PreClassName}\">", "</pre>"),
                new Tag("spoiler",
                    $"<a href=\"javascript:void(0)\" class=\"{SpoilerHeadClassName}\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"{SpoilerClassName}\">",
                    "</div>"),
                new Tag("img",
                    $"<a href=\"{{value}}\" target=\"_blank\"><img src=\"{{value}}\" class=\"{ImageClassName}\" /></a>",
                    true),
                new Tag("link", "<a href=\"{value}\">", "</a>", true),
                new Tag("quote",
                    $"<div class=\"{QuoteClassName}\"><div class=\"{QuoteAuthorClassName}\">{{value}}</div>",
                    "</div>", true, AttributeEscapeMode.Html),
                new Tag("tab", "&nbsp;&nbsp;&nbsp;"),
                new Tag("private", "{value}", "", true, AttributeEscapeMode.Html),
                new CodeTag("code", $"<pre class=\"{CodeClassName}\">", "</pre>"),
                new ListTag("ul", "<ul>", "</ul>"),
                new Tag("li", "<li>", "</li>"),
            ],
            BBParser.SecuritySubstitutions,
            new Dictionary<string, string>
            {
                {"---", "&mdash;"},
                {"--", "&ndash;"},
                {"\r\n", "<br />"}
            });
    }

    [Test]
    public void TestParseSimple()
    {
        const string input = "[b]hello[/b]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<strong>hello</strong>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestParseWithMoreClosingTags()
    {
        var actual = _bbCodeParser.Parse("[b]hello[/b][/b][/b]");
        Assert.That(actual.ToHtml(), Is.EqualTo("<strong>hello</strong>"));
    }

    [Test]
    public void TestParseSimpleLinear()
    {
        var actual = _bbCodeParser.Parse("[b]hello[/b] [i]world[/i]");
        Assert.That(actual.ToHtml(), Is.EqualTo("<strong>hello</strong> <em>world</em>"));
    }

    [Test]
    public void TestParseSimpleTree()
    {
        var actual = _bbCodeParser.Parse("[b]hello [i]world[/i][/b]");
        Assert.That(actual.ToHtml(), Is.EqualTo("<strong>hello <em>world</em></strong>"));
    }

    [Test]
    public void TestParseInvalidTree()
    {
        var actual1 = _bbCodeParser.Parse("[b]hello [i]world[/b][/i]");
        var actual2 = _bbCodeParser.Parse("[b]hello [i]world[/i][/i]");
        var actual3 = _bbCodeParser.Parse("[b]hello [i]world[/b][/b]");
        const string expected = "<strong>hello <em>world</em></strong>";
        Assert.That(actual1.ToHtml(), Is.EqualTo(expected));
        Assert.That(actual2.ToHtml(), Is.EqualTo(expected));
        Assert.That(actual3.ToHtml(), Is.EqualTo(expected));
    }

    [Test]
    public void TestLink()
    {
        const string input = "[link=\"hello\"]linktext[/link]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"hello\">linktext</a>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestImage()
    {
        const string input = "[img=\"hello\"]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(),
            Is.EqualTo("<a href=\"hello\" target=\"_blank\"><img src=\"hello\" class=\"image\" /></a>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestTab()
    {
        const string input = "hello[tab]bye";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("hello&nbsp;&nbsp;&nbsp;bye"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestFail()
    {
        const string input = "[[b]aaa[/b]]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("[<strong>aaa</strong>]"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestSpoiler()
    {
        const string input = "[link=\"http://yandex.ru\"]Яндекс[/link][spoiler]hehehe[/spoiler]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(
            actual.ToHtml(),
            Is.EqualTo("<a href=\"http://yandex.ru\">Яндекс</a><a href=\"javascript:void(0)\" class=\"spoiler-head\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"spoiler\">hehehe</div>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestOptionalAttribute()
    {
        const string input1 = "[quote=\"Author\"]test[/quote]";
        var actual1 = _bbCodeParser.Parse(input1);
        Assert.That(actual1.ToHtml(),
            Is.EqualTo("<div class=\"quote\"><div class=\"quote-author\">Author</div>test</div>"));
        Assert.That(actual1.ToBb(), Is.EqualTo(input1));

        const string input2 = "[quote]test[/quote]";
        var actual2 = _bbCodeParser.Parse(input2);
        Assert.That(actual2.ToHtml(), Is.EqualTo("<div class=\"quote\"><div class=\"quote-author\"></div>test</div>"));
        Assert.That(actual2.ToBb(), Is.EqualTo(input2));
    }

    [Test]
    public void TestWithAttribute()
    {
        const string input = "[spoiler][img=\"https://imgurl.jpg\"][/spoiler]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(
            actual.ToHtml(),
            Is.EqualTo("<a href=\"javascript:void(0)\" class=\"spoiler-head\" data-swaptext=\"Скрыть содержимое\">Показать содержимое</a><div class=\"spoiler\"><a href=\"https://imgurl.jpg\" target=\"_blank\"><img src=\"https://imgurl.jpg\" class=\"image\" /></a></div>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestCode()
    {
        const string input = "[b]Not inside the code[/b]... [i]not yet[/i]. [code]And [b]this one is[/b]<script></script>[/code]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(
            actual.ToHtml(),
            Is.EqualTo("<strong>Not inside the code</strong>... <em>not yet</em>. <pre class=\"code\">And [b]this one is[/b]&lt;script&gt;&lt;/script&gt;</pre>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestAliasInCode()
    {
        const string input = "--- [b]Not inside the code[/b]... [i]not yet[/i]. [code]And --- [b]this --- one is[/b]<script>---</script>[/code]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(
            actual.ToHtml(),
            Is.EqualTo("&mdash; <strong>Not inside the code</strong>... <em>not yet</em>. <pre class=\"code\">And --- [b]this --- one is[/b]&lt;script&gt;---&lt;/script&gt;</pre>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestJsInjection()
    {
        const string input = "[link=\"http://yandex.ru\' onload='alert\"]coolhack[/link]\"";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"http://yandex.ruonload=alert\">coolhack</a>\""));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestHtmlInjection()
    {
        const string input = "[b]<script>console.log('hi');</script>&nbsp;[/b]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(),
            Is.EqualTo("<strong>&lt;script&gt;console.log('hi');&lt;/script&gt;&amp;nbsp;</strong>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestBugWithTextAfterEndingTag()
    {
        const string input = "test![b]Test [link=\"url\"]link[/link]text[/b]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("test!<strong>Test <a href=\"url\">link</a>text</strong>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestJsInjectionWithExecuting()
    {
        const string input = "[link=\"Javascript:alert(`XSS`)\"]alert[/link]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"_xss_alert(XSS)\">alert</a>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestBugWithInvalidTree()
    {
        const string input = "test![b]Test[i]1[/b]test";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("test!<strong>Test<em>1</em></strong>test"));
        Assert.That(actual.ToBb(), Is.EqualTo("test![b]Test[i]1[/i][/b]test"));
    }

    [Test]
    [Ignore("Not sure why this is considered dangerous; this [link] is not converted to <a> at all")]
    public void TestBugWithHiddenXss()
    {
        var input = "[link=\"JaVas\"C'ript:alert(document.cookie)\"]aaa[/link]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"_xss_alert(document.cookie)\">aaa</a>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestXss1()
    {
        const string input = "[link=\"javascript&'#058;alert(/xss/)\"]ddd[/link]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"javascriptalert(/xss/)\">ddd</a>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestXss2()
    {
        const string input = "[link=\"Javas&'#x09;cript:alert(/xss/)\"]123[/link]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"_xss_alert(/xss/)\">123</a>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    [Ignore("Not sure why this is considered dangerous; this [link] is not converted to <a> at all")]
    public void TestXss3()
    {
        var input =
            "[link=\"javascriptJa vA s\"'\"'    \"'''\"\"\"\"   cript::alert(\"/Preved, admincheg/\")\"]Preved[/link]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"javascript_xss_:alert(/Preved,admincheg/)\">Preved</a>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    [Ignore("Not sure why this is considered dangerous; this [link] is not converted to <a> at all")]
    public void TestXss4()
    {
        var input = "[link=\"javasc&\"#0000009ript:alert(/xss/)\"]ddd[/link]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"javasc:alert(/xss/)\">ddd</a>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestCodeResolvingInnerBbIssues()
    {
        const string input = "[code][i]test[/code]";
        var actual = _bbCodeParser.Parse(input);

        Assert.That(actual.ToHtml(), Is.EqualTo("<pre class=\"code\">[i]test</pre>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestNonTaggedInputsBug()
    {
        const string input = "[notaspoi...\"\r\ntest [s]test[/s]";
        var actual = _bbCodeParser.Parse(input);

        Assert.That(actual.ToHtml(), Is.EqualTo("[notaspoi...\"<br />test <s>test</s>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestNullReferenceFallbackBug1()
    {
        const string input = @"Порядок тегов имеет значение при использовании тега head (проверял в [link=""http://dungeonmaster.ru.trioptimum.ru/profile/Hatchet""]своем профиле[/link])\r\n\r\n1. [code][head][b]Lorem Ipsum[/b][/head][/code] - на выходе получаем полужирный заголовок\r\n2. [code][b][head]Lorem Ipsum[/head][/b][/code] - на выходе получаем просто заголовок\r\nВ то же время для остальных тегов - b, i, s, u - порядок вложения значения не имеет.\r\n\r\nОжидание: оба варианта выше должны работать одинаково.\r\n\r\nP.S. А в [link=""http://dungeonmaster.ru.trioptimum.ru/parsertest""]тесте парсера[/link] тег head вообще не работает";
        NodeTree? actual = null;
        Assert.DoesNotThrow(delegate { actual = _bbCodeParser.Parse(input); });
        Assert.DoesNotThrow(delegate { actual?.ToHtml(); });
        Assert.DoesNotThrow(delegate { actual?.ToBb(); });
        Assert.DoesNotThrow(delegate { actual?.ToText(); });
    }

    [Test]
    public void TestNullReferenceFallbackBug2()
    {
        const string input = "[spoiler][img=\"http://s05.radikal.ru/i178/1609/96/a9db8fef8310.png\"][/spoiler][spoiler][img=\"http://s019.radikal.ru/i614/1609/e0/0cf83e802ef7.png\"][/spoiler]\r\nКогда ставятся теги спойлера и цитаты, они отображаются на тексте с лишней пустой строкой, если писать текст начинать не сразу же после тега, а в другой строке.\r\n[code][spoiler]Спойлер[/spoiler]текст сразу после тега[/code] преобразится в \r\n\"скрыть содержимое\r\nтекст сразу после тега\"\r\n\r\nТо же самое с цитатой. \r\n\r\nНо если сделать:\r\n[code][spoiler]Спойлер[/spoiler]\r\nТекст в другой строке после тега[/code]\r\nоно преобразится в \r\n\"показать содержимое\r\n\r\nТекст в другой строке после тега\"\r\nТ.е., появится пустая строка. Это очень неудобно при форматировании текста, приходится постоянно учитывать этот момент, что появится пустая строка. А если хочешь сделать цепочку без разрывов:\r\n\"текст\r\nспойлер\r\nтекст\r\nспойлер и тд\"\r\nТо её нужно записать сплошным текстом:  \"текст[спойлер]текст[спойлер]текст[спо...\", что опять же неудобно.\r\n\r\n[i]Предложение[/i]: не вставлять эту пустую строку у тегов спойлер и цитата, если текст после тега начинается в другой/других строке/строках.\r\n\r\nТ.е.\r\n\"[code][spoiler]Спойлер[/spoiler]текст сразу после тега[/code] \"\r\nпреобразится в \r\n\"скрыть содержимое\r\nтекст сразу после тега\"\r\n\r\n\"[code][spoiler]Спойлер[/spoiler]\r\nтекст в другой строке после тега[/code] \"\r\nпреобразится в \r\n\"скрыть содержимое\r\nтекст сразу после тега\"\r\n\r\n\"[code][spoiler]Спойлер[/spoiler]\r\n\r\nтекст во второй строке после тега[/code] \"\r\nпреобразится в\r\n\"скрыть содержимое\r\n\r\nтекст во второй строке после тега\"\r\n\r\nUPD: почему-то не хочет \"предложение\" выделяться курсивом. Делаю его [code][i]Предложение[/i]:[/code], а сохраняется \"[code][_i]Предложение: ([_/i] вот это тут само вставляется в конце, если убрать подчёркивание и оставить один [_i])[/code]\"";
        NodeTree? actual = null;
        Assert.DoesNotThrow(delegate { actual = _bbCodeParser.Parse(input); });
        Assert.DoesNotThrow(delegate { actual?.ToHtml(); });
        Assert.DoesNotThrow(delegate { actual?.ToBb(); });
        Assert.DoesNotThrow(delegate { actual?.ToText(); });
    }

    [Test]
    public void TestBugWithDoublePrivate()
    {
        const string input = "[code][private=\"Test\"]Test[/private][/code]";
        var actual = _bbCodeParser.Parse(input);
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestFilter()
    {
        const string input1 = "[private=\"Test1, Test2\"]Hi[/private]";
        const string input2 = "[private=\"Test1, Test3\"]Hi[/private]";
        const string input3 = "[private=\"Test1, Test2, Test3\"]Hi[/private]";
        var actual1 = _bbCodeParser.Parse(input1);
        var actual2 = _bbCodeParser.Parse(input2);
        var actual3 = _bbCodeParser.Parse(input3);

        Func<Node, bool> filter = n => n is not TagNode || (((TagNode?)n)?.AttributeValue ?? string.Empty).Split(',')
            .Select(v => v.Trim()).Any(v =>
                v.Equals("Test3", StringComparison.OrdinalIgnoreCase));
        Func<Node, string?, string> filterAttributeValue = (v, n) => "Test3";

        ClassicAssert.IsEmpty(actual1.ToHtml(filter, filterAttributeValue));

        Assert.That(actual2.ToHtml(filter, filterAttributeValue), Is.EqualTo("Test3Hi"));
        Assert.That(actual2.ToHtml(filter), Is.EqualTo("Test1, Test3Hi"));

        Assert.That(actual3.ToHtml(filter, filterAttributeValue), Is.EqualTo("Test3Hi"));
        Assert.That(actual3.ToHtml(filter), Is.EqualTo("Test1, Test2, Test3Hi"));
    }

    [Test]
    public void TestAliasSubstitutions()
    {
        const string input1 = "Hello< --[code] [b]&world --->[/code]";
        const string input2 = "Hello< --[pre] [b]&world --->[/pre]";
        var actual1 = _bbCodeParser.Parse(input1);
        var actual2 = _bbCodeParser.Parse(input2);

        Assert.That(actual1.ToHtml(), Is.EqualTo("Hello&lt; &ndash;<pre class=\"code\"> [b]&amp;world ---&gt;</pre>"));
        Assert.That(actual1.ToBb(), Is.EqualTo(input1));

        Assert.That(actual2.ToHtml(),
            Is.EqualTo("Hello&lt; &ndash;<pre class=\"pref\"> <strong>&amp;world ---&gt;</strong></pre>"));
        Assert.That(actual2.ToBb(), Is.EqualTo("Hello< --[pre] [b]&world --->[/b][/pre]"));
    }

    [Test]
    public void TestToBbAndTextFilters()
    {
        const string input = "[i]Hello[/i] [b]world[/b]";
        var actual = _bbCodeParser.Parse(input);

        Func<Node, bool> filter = n => n is not TagNode || ((TagNode?) n)?.Tag?.Name != "b";
        Assert.That(actual.ToBb(filter), Is.EqualTo("[i]Hello[/i] "));
        Assert.That(actual.ToText(filter), Is.EqualTo("Hello "));
    }

    [Test]
    public void TestToBbAndTextFiltersWithCode()
    {
        const string input = "[i]Hello[/i] [code][b]world[/b][/code]";
        var actual = _bbCodeParser.Parse(input);

        static bool Filter(Node n) => n is not TagNode || ((TagNode?) n)?.Tag?.Name != "b";
        Assert.That(actual.ToBb(Filter), Is.EqualTo("[i]Hello[/i] [code][b]world[/b][/code]"));
        Assert.That(actual.ToText(Filter), Is.EqualTo("Hello [b]world[/b]"));
    }

    [Test]
    public void TestListCode()
    {
        const string input = @"[ul]
[li]test1[/li]
[li]test2[/li][/ul]";
        var actual = _bbCodeParser.Parse(input);

        Assert.That(actual.ToHtml(), Is.EqualTo("<ul><li>test1</li><li>test2</li></ul>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestListCode2()
    {
        const string input = @"[ul][li]чи шо[/li]
asdfas
[li][/li][/ul]";
        var actual = _bbCodeParser.Parse(input);

        Assert.That(actual.ToHtml(), Is.EqualTo("<ul><li>чи шо</li><li></li></ul>"));
        Assert.That(actual.ToBb(), Is.EqualTo(input));
    }

    [Test]
    public void TestRecursiveXss()
    {
        const string input = "[link=\"jav&#&#90;90;ascript:\"]test[/link]";
        var actual = _bbCodeParser.Parse(input);

        Assert.That(actual.ToHtml(), Is.EqualTo("<a href=\"_xss_\">test</a>"));
    }

    [Test]
    public void TestHtmlXssInAttributes()
    {
        const string input = "[quote=\"<script>alert('xss');</script>\"]test[/quote]";
        var actual = _bbCodeParser.Parse(input);

        Assert.That(actual.ToHtml(), Is.EqualTo("<div class=\"quote\"><div class=\"quote-author\">&lt;script&gt;alert('xss');&lt;/script&gt;</div>test</div>"));
    }
}
