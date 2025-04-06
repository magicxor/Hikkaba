using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the minimum number of files in a file collection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class MinFileCountAttribute : ValidationAttribute
{
    public int MinFileCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinFileCountAttribute"/> class.
    /// </summary>
    /// <param name="minFileCount">Minimum number of files.</param>
    public MinFileCountAttribute(int minFileCount)
    {
        MinFileCount = minFileCount;
    }

    /// <summary>
    /// Validate that the number of files in the collection is equal or greater than the specified minimum or that it is null.
    /// </summary>
    /// <param name="value">Object to validate.</param>
    /// <param name="validationContext">Context in which a validation check is performed.</param>
    /// <returns>True if the number of files is equal or greater than <see cref="MinFileCount"/> or if it is null.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFileCollection formFileCollection)
        {
            var fileCount = formFileCollection.Count;

            if (fileCount < MinFileCount)
                return new ValidationResult($"At least {MinFileCount} file(s) required. Actual: {fileCount}.");

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
