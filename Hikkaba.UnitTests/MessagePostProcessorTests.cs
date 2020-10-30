using Hikkaba.Web.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.UnitTests
{
    public class MessagePostProcessorTests
    {
        private IUrlHelperFactoryWrapper _wrapper;

        [SetUp]
        public void Setup()
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(helper => helper.Action(It.IsAny<UrlActionContext>()))
                .Returns("some_action_link");
            
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
            Assert.IsTrue(text.Equals(result), $"{nameof(text)} {text} != {nameof(result)} {result}");
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
            var comparisonResult = text.Equals(result);
            if (mustBeEqual)
            {
                Assert.IsTrue(comparisonResult, $"{nameof(text)} {text} != {nameof(result)} {result}");
            }
            else
            {
                Assert.IsFalse(comparisonResult, $"{nameof(text)} {text} == {nameof(result)} {result}");
            }
        }

        [TestCase("[b]bold[/b]", "<b>bold</b>")]
        [TestCase("http://example.com", "<a href=\"http://example.com\">http://example.com</a>")]
        [TestCase("https://example.com", "<a href=\"https://example.com\">https://example.com</a>")]
        [TestCase("ftp://example.com", "<a href=\"ftp://example.com\">ftp://example.com</a>")]
        [TestCase(">>abc", "<a href=\"some_action_link#abc\">&gt;&gt;abc</a>")]
        [TestCase(">>0", "<a href=\"some_action_link#0\">&gt;&gt;0</a>")]
        [TestCase(">>999", "<a href=\"some_action_link#999\">&gt;&gt;999</a>")]
        public void TestTransformations(string source, string expectedResult)
        {
            var messagePostProcessor = new MessagePostProcessor(_wrapper);
            var result = messagePostProcessor.Process("a", Guid.Parse("d133c970-580f-4926-9588-3f49bb914162"), source);
            Assert.IsTrue(result.Equals(expectedResult), $"{nameof(result)} {result} != {nameof(expectedResult)} {expectedResult}");
        }
    }
}
