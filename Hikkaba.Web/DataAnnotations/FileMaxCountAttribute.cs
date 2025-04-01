using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FileMaxCountAttribute : ValidationAttribute
{
    private readonly long _maxFileCount;

    public FileMaxCountAttribute(int maxFileCount)
    {
        _maxFileCount = maxFileCount;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFileCollection formFileCollection)
        {
            var fileCount = formFileCollection.Count;

            if (fileCount > _maxFileCount)
                return new ValidationResult($"Maximum allowed file count: {_maxFileCount}. Actual: {fileCount}.");

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
