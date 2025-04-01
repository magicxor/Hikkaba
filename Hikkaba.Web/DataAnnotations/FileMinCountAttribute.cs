using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FileMinCountAttribute : ValidationAttribute
{
    private readonly long _minFileCount;

    public FileMinCountAttribute(int minFileCount)
    {
        _minFileCount = minFileCount;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFileCollection formFileCollection)
        {
            var fileCount = formFileCollection.Count;

            if (fileCount < _minFileCount)
                return new ValidationResult($"At least {_minFileCount} file(s) required. Actual: {fileCount}.");

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
