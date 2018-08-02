using System;
using Hikkaba.Web.Binding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Hikkaba.Web.Binding.Providers
{
    public class DateTimeKindSensitiveBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            else if ((!context.Metadata.IsComplexType) && (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?)))
            {
                return new DateTimeKindSensitiveBinder(context.Metadata.ModelType);
            }
            else
            {
                return null;
            }
        }
    }
}
