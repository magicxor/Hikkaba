using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Hikkaba.Web.Services.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hikkaba.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDntCaptchaMock(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IDNTCaptchaValidatorService, DntCaptchaMock>()
            .AddSingleton<ICaptchaProtectionProvider, CaptchaProtectionProviderMock>()
            .AddSingleton<IRandomNumberProvider, RandomNumberProviderMock>()
            .AddSingleton<ICaptchaTextProvider, CaptchaTextProviderMock>()
            .AddSingleton<Func<DisplayMode, ICaptchaTextProvider>>(_ => _ => new CaptchaTextProviderMock())
            .AddSingleton<ICaptchaStorageProvider, CaptchaStorageProviderMock>()
            .AddSingleton<ISerializationProvider, InMemorySerializationProvider>()
            .AddSingleton<ICaptchaImageProvider, CaptchaImageProviderMock>();
    }
}