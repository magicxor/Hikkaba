using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class AtLeastOnePropertyAttribute : ValidationAttribute
{
    public IReadOnlyList<string> PropertyList { get; }

    public AtLeastOnePropertyAttribute(params string[] propertyList)
    {
        PropertyList = propertyList;
    }

    // see http://stackoverflow.com/a/1365669
    public override object TypeId => this;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        bool HasData(string propertyName)
        {
            var propertyInfo = value?.GetType().GetProperty(propertyName);

            if (propertyInfo == null)
            {
                return false;
            }

            if (propertyInfo.PropertyType.IsAssignableTo(typeof(IFormFile)))
            {
                return propertyInfo.GetValue(value, null) is IFormFile { Length: > 0 };
            }

            if (propertyInfo.PropertyType.IsAssignableTo(typeof(IFormFileCollection)))
            {
                return propertyInfo.GetValue(value, null) is IFormFileCollection { Count: > 0 };
            }

            if (propertyInfo.PropertyType == typeof(string))
            {
                var stringValue = propertyInfo.GetValue(value, null) as string;
                return !string.IsNullOrEmpty(stringValue);
            }

            if (propertyInfo.GetValue(value, null) != null)
            {
                return true;
            }

            return false;
        }

        var errorMessage = ErrorMessage ?? $"At least one property must be set: {string.Join(", ", PropertyList)}";

        return PropertyList.Any(HasData)
            ? ValidationResult.Success
            : new ValidationResult(
                string.Format(CultureInfo.InvariantCulture, errorMessage, string.Join(", ", PropertyList)),
                PropertyList);
    }
}
