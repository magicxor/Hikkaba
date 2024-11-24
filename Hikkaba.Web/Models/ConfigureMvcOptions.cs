using Hikkaba.Web.Binding.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Hikkaba.Web.Models;

public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    private readonly DateTimeKindSensitiveBinderProvider _dateTimeKindSensitiveBinderProvider;

    public ConfigureMvcOptions(DateTimeKindSensitiveBinderProvider dateTimeKindSensitiveBinderProvider)
    {
        _dateTimeKindSensitiveBinderProvider = dateTimeKindSensitiveBinderProvider;
    }

    public void Configure(MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, _dateTimeKindSensitiveBinderProvider);
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    }
}
