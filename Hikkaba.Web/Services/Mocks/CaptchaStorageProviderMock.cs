using DNTCaptcha.Core.Contracts;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.Services.Mocks;

public class CaptchaStorageProviderMock : ICaptchaStorageProvider
{
    public void Add(HttpContext context, string token, string value)
    {
    }

    public bool Contains(HttpContext context, string token)
    {
        return true;
    }

    public string GetValue(HttpContext context, string token)
    {
        return string.Empty;
    }

    public void Remove(HttpContext context, string token)
    {
    }
}