using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the maximum number of files in a file collection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MaxFileCountAttribute : ValidationAttribute
{
    private readonly long _maxFileCount;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="maxFileCount">Maximum number of files.</param>
    public MaxFileCountAttribute(int maxFileCount)
    {
        _maxFileCount = maxFileCount;
    }

    /// <summary>
    /// Validate that the number of files in the collection is equal or smaller than the specified maximum or that it is null.
    /// </summary>
    /// <param name="value">Object to validate.</param>
    /// <param name="validationContext">Context in which a validation check is performed.</param>
    /// <returns>True if the number of files is equal or smaller than <see cref="_maxFileCount"/> or if it is null.</returns>
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
