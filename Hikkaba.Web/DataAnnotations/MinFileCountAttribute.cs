using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the minimum number of files in a file collection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MinFileCountAttribute : ValidationAttribute
{
    private readonly long _minFileCount;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="minFileCount">Minimum number of files.</param>
    public MinFileCountAttribute(int minFileCount)
    {
        _minFileCount = minFileCount;
    }

    /// <summary>
    /// Validate that the number of files in the collection is equal or greater than the specified minimum or that it is null.
    /// </summary>
    /// <param name="value">Object to validate.</param>
    /// <param name="validationContext">Context in which a validation check is performed.</param>
    /// <returns>True if the number of files is equal or greater than <see cref="_minFileCount"/> or if it is null.</returns>
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
