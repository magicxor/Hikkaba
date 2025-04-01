using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the maximum total size of a IFormFileCollection object.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FileCollectionSizeMaxAttribute : ValidationAttribute
{
    /// <summary>
    /// Maximum total size in bytes.
    /// </summary>
    private readonly long _maxFileCollectionSize;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="maxFileCollectionSize">Maximum total size in bytes.</param>
    public FileCollectionSizeMaxAttribute(long maxFileCollectionSize)
    {
        _maxFileCollectionSize = maxFileCollectionSize;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFileCollection formFileCollection)
        {
            var totalSize = formFileCollection.Sum(formFile => formFile.Length);

            if (totalSize > _maxFileCollectionSize)
                return new ValidationResult($"Maximum allowed total file size: {_maxFileCollectionSize} bytes. Actual: {totalSize}.");

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
