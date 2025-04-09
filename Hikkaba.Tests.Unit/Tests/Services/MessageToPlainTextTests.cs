using System;
using Hikkaba.Tests.Unit.Mocks;
using Hikkaba.Web.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class MessageToPlainTextTests
{
    private const string FakeActionPath = "/b/23454362";
    private static readonly FakeUrlHelperParams FakeUrlHelperParams = new()
    {
        Action = FakeActionPath,
        RouteUrlFactory = (routeName, values) => routeName == "ThreadDetails"
            ? FakeActionPath
            : throw new ArgumentException("Invalid route name", nameof(routeName)),
    };

    [TestCase("ＳＯＭＥ　ＳＴＲＡＮＧＥ　ＴＥＸＴ，　ｈｕｈ　^^　延凹線艶彙")]
    [TestCase("Ｓ♢ＭΞ░ＳＴＲΛＮＧΞ░ＴΞＸＴ，░ｈｕｈ░^^　（延凹線艶彙）")]
    [TestCase("【﻿ＳＯＭＥ　ＳＴＲＡＮＧＥ　ＴＥＸＴ，　ｈｕｈ　^^】")]
    [TestCase("丂ㄖ爪乇　丂ㄒ尺卂几Ꮆ乇　ㄒ乇乂ㄒ，　卄ㄩ卄　^^")]
    [TestCase("己回冊ヨ　己卞尺丹几呂ヨ　卞ヨメ卞，　廾凵廾　^^")]
    [TestCase("丂のﾶ乇　丂ｲ尺ﾑ刀ム乇　ｲ乇ﾒｲ，　んひん　^^")]
    [TestCase("Some letters. 1234567890; 987 * 2 - 5 @! | [wow](!wow)[!wow][[ yoy )))) [[[ ]] ] \\ //.")]
    [TestCase("<div>Some text</div>")]
    [TestCase("Line 1\nLine 2\nLine 3")]
    [TestCase("Line 1\n\nLine 2\n\nLine 3")]
    public void MessageToPlainText_WhenCalledWithText_ShouldReturnTheSameText(string input)
    {
        using var customAppFactory = new CustomAppFactory(FakeUrlHelperParams);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToPlainText(input);
        Assert.That(actualOutput, Is.EqualTo(input));
    }

    [TestCase("[b]bold[/b]", "bold")]
    [TestCase("[i]italic[/i]", "italic")]
    [TestCase("[u]underline[/u]", "underline")]
    [TestCase("[s]strikethrough[/s]", "strikethrough")]
    [TestCase("plain [b][i][u]mix3[/u] mix2[/i] mix1[/b] plain", "plain mix3 mix2 mix1 plain")]
    [TestCase("""
              plain [b][i][u]
              mix3[/u]
              mix2[/i] mix1[/b]
              plain
              """, "plain \nmix3\nmix2 mix1\nplain")]
    public void MessageToPlainText_WhenCalledWithTags_ShouldReturnPlainText(string input, string expectedOutput)
    {
        using var customAppFactory = new CustomAppFactory(FakeUrlHelperParams);
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var messagePostProcessor = scope.ServiceProvider.GetRequiredService<IMessagePostProcessor>();

        var actualOutput = messagePostProcessor.MessageToPlainText(input);
        Assert.That(actualOutput, Is.EqualTo(expectedOutput));
    }
}
