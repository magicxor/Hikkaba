using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Binding.Binders;

public class DateTimeKindSensitiveBinder : IModelBinder
{
    private readonly SimpleTypeModelBinder _baseBinder;

    public DateTimeKindSensitiveBinder(Type type, ILoggerFactory loggerFactory)
    {
        _baseBinder = new SimpleTypeModelBinder(type, loggerFactory);
    }

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        // Check the value sent in
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult != ValueProviderResult.None)
        {
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            if (DateTime.TryParse(valueProviderResult.FirstValue, null, DateTimeStyles.RoundtripKind, out var model))
            {
                bindingContext.Result = ModelBindingResult.Success(model);
                return Task.CompletedTask;
            }
        }

        // If we haven't handled it, then we'll let the base SimpleTypeModelBinder handle it
        return _baseBinder.BindModelAsync(bindingContext);
    }
}
