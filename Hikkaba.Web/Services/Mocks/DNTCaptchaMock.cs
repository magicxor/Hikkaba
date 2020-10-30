using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;

namespace Hikkaba.Web.Services.Mocks
{
    public class DNTCaptchaMock : IDNTCaptchaValidatorService
    {
        public bool HasRequestValidCaptchaEntry(Language captchaGeneratorLanguage, DisplayMode captchaGeneratorDisplayMode, DNTCaptchaBase model = null)
        {
            return true;
        }
    }
}
