﻿using System;
using Hikkaba.Web.Binding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Binding.Providers;

internal class DateTimeKindSensitiveBinderProvider : IModelBinderProvider
{
    private readonly ILoggerFactory _loggerFactory;

    public DateTimeKindSensitiveBinderProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (!context.Metadata.IsComplexType
            && (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?)))
        {
            return new DateTimeKindSensitiveBinder(context.Metadata.ModelType, _loggerFactory);
        }

        return null;
    }
}
