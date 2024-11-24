using Hikkaba.Web.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Tests.Unit;

public class MessagePostProcessorTests
{
    private const string ActionLinkExample = "/b/Threads/1dcc5435-f3bd-43eb-97b3-155f34206514";

    private IUrlHelperFactoryWrapper _wrapper;

    [SetUp]
    public void Setup()
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(helper => helper.Action(It.IsAny<UrlActionContext>()))
            .Returns(ActionLinkExample);

        var wrapper = new Mock<IUrlHelperFactoryWrapper>();
        wrapper.Setup(w => w.GetUrlHelper())
            .Returns(urlHelper.Object);
        _wrapper = wrapper.Object;
    }

    [TestCase("ＳＯＭＥ　ＳＴＲＡＮＧＥ　ＴＥＸＴ，　ｈｕｈ　^^　延凹線艶彙")]
    [TestCase("Ｓ♢ＭΞ░ＳＴＲΛＮＧΞ░ＴΞＸＴ，░ｈｕｈ░^^　（延凹線艶彙）")]
    [TestCase("【﻿ＳＯＭＥ　ＳＴＲＡＮＧＥ　ＴＥＸＴ，　ｈｕｈ　^^】")]
    [TestCase("丂ㄖ爪乇　丂ㄒ尺卂几Ꮆ乇　ㄒ乇乂ㄒ，　卄ㄩ卄　^^")]
    [TestCase("己回冊ヨ　己卞尺丹几呂ヨ　卞ヨメ卞，　廾凵廾　^^")]
    [TestCase("丂のﾶ乇　丂ｲ尺ﾑ刀ム乇　ｲ乇ﾒｲ，　んひん　^^")]
    [TestCase(
        @"TEXT
WITH
LINE
BREAKS")]
    public void TestPlainText(string text)
    {
        var messagePostProcessor = new MessagePostProcessor(_wrapper);
        var result = messagePostProcessor.Process("a", Guid.Parse("d133c970-580f-4926-9588-3f49bb914162"), text);
        Assert.That(result, Is.EqualTo(text));
    }

    [TestCase("TEXT\r\nWITH\r\nLINE\r\nBREAKS", true)]
    [TestCase("TEXT\r\n\r\n\r\nWITH LINE BREAKS", false)]
    [TestCase("TEXT\r\nWITH LINE BREAKS", true)]
    [TestCase("TEXT\nWITH LINE BREAKS", false)]
    [TestCase("TEXT\n\nWITH LINE BREAKS", false)]
    [TestCase("TEXT\n\rWITH LINE BREAKS", false)]
    public void TestPlainTextWithLineBreaks(string text, bool mustBeEqual)
    {
        var messagePostProcessor = new MessagePostProcessor(_wrapper);
        var result = messagePostProcessor.Process("a", Guid.Parse("d133c970-580f-4926-9588-3f49bb914162"), text);
        Assert.That(result, mustBeEqual ? Is.EqualTo(text) : Is.Not.EqualTo(text));
    }

    [TestCase("[b]bold[/b]", "<b>bold</b>")]
    [TestCase("http://example.com", "<a href=\"http://example.com\" rel=\"nofollow noopener noreferrer external\">http://example.com</a>")]
    [TestCase("https://example.com", "<a href=\"https://example.com\" rel=\"nofollow noopener noreferrer external\">https://example.com</a>")]
    [TestCase("ftp://example.com", "<a href=\"ftp://example.com\" rel=\"nofollow noopener noreferrer external\">ftp://example.com</a>")]
    [TestCase("http://example.com/item/a-b-c/1823888278.html?spm=2114.30010708.3.17.2rt7qZ&ws_ab_test=searchweb201556_8,searchweb201602_", "<a href=\"http://example.com/item/a-b-c/1823888278.html?spm=2114.30010708.3.17.2rt7qZ&amp;ws_ab_test=searchweb201556_8,searchweb201602_\" rel=\"nofollow noopener noreferrer external\">http://example.com/item/a-b-c/1823888278.html?spm=2114.30010708.3.17.2rt7qZ&amp;ws_ab_test=searchweb201556_8,searchweb201602_</a>")]
    [TestCase(">>abc", "<a href=\""+ ActionLinkExample +"#abc\">&gt;&gt;abc</a>")]
    [TestCase(">>0", "<a href=\""+ ActionLinkExample +"#0\">&gt;&gt;0</a>")]
    [TestCase(">>999", "<a href=\""+ ActionLinkExample +"#999\">&gt;&gt;999</a>")]
    public void TestTransformations(string source, string expectedResult)
    {
        var messagePostProcessor = new MessagePostProcessor(_wrapper);
        var result = messagePostProcessor.Process("a", Guid.Parse("d133c970-580f-4926-9588-3f49bb914162"), source);
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
