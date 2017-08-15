using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hikkaba.Web.Binding.Binders
{
    public class DateTimeKindSensitiveBinder : IModelBinder
    {
        SimpleTypeModelBinder _baseBinder;

        public DateTimeKindSensitiveBinder(Type type)
        {
            _baseBinder = new SimpleTypeModelBinder(type);
        }

        /// <inheritdoc />
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null){ throw new ArgumentNullException(nameof(bindingContext)); }

            // Check the value sent in
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult); // todo: ???

                DateTime model;
                if (DateTime.TryParse(valueProviderResult.FirstValue, null, DateTimeStyles.RoundtripKind, out model))
                {
                    bindingContext.Result = ModelBindingResult.Success(model);
                    return Task.CompletedTask;
                }
            }

            // If we haven't handled it, then we'll let the base SimpleTypeModelBinder handle it
            return _baseBinder.BindModelAsync(bindingContext);
        }
    }
}
