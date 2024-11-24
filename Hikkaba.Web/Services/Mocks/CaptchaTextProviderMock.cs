using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;

namespace Hikkaba.Web.Services.Mocks;

public class CaptchaTextProviderMock : ICaptchaTextProvider
{
    public string GetText(long number, Language language)
    {
        return string.Empty;
    }
}