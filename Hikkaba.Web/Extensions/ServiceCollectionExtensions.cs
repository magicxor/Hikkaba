using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Hikkaba.Web.Services.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hikkaba.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDNTCaptchaMock(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<IDNTCaptchaValidatorService, DNTCaptchaMock>()
                .AddSingleton<ICaptchaProtectionProvider, CaptchaProtectionProviderMock>()
                .AddSingleton<IRandomNumberProvider, RandomNumberProviderMock>()
                .AddSingleton<ICaptchaTextProvider, CaptchaTextProviderMock>()
                .AddSingleton<Func<DisplayMode, ICaptchaTextProvider>>(serviceProvider => key => new CaptchaTextProviderMock())
                .AddSingleton<ICaptchaStorageProvider, CaptchaStorageProviderMock>()
                .AddSingleton<ISerializationProvider, InMemorySerializationProvider>()
                .AddSingleton<ICaptchaImageProvider, CaptchaImageProviderMock>();
        }
    }
}
