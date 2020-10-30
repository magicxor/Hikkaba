using DNTCaptcha.Core.Contracts;

namespace Hikkaba.Web.Services.Mocks
{
    public class CaptchaProtectionProviderMock : ICaptchaProtectionProvider
    {
        public string Decrypt(string inputText)
        {
            return string.Empty;
        }

        public string Encrypt(string inputText)
        {
            return string.Empty;
        }

        public string Hash(string inputText)
        {
            return string.Empty;
        }
    }
}
