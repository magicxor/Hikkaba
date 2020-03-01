using Hikkaba.Web.Binding.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hikkaba.Web.Models
{
    public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
    {
        private readonly ILogger<MvcOptions> _logger;
        private readonly DateTimeKindSensitiveBinderProvider _dateTimeKindSensitiveBinderProvider;

        public ConfigureMvcOptions(ILogger<MvcOptions> logger, DateTimeKindSensitiveBinderProvider dateTimeKindSensitiveBinderProvider)
        {
            _logger = logger;
            _dateTimeKindSensitiveBinderProvider = dateTimeKindSensitiveBinderProvider;
        }

        public void Configure(MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, _dateTimeKindSensitiveBinderProvider);
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        }
    }
}
