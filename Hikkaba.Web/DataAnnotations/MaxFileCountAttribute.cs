using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the maximum number of files in a file collection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class MaxFileCountAttribute : ValidationAttribute
{
    public int MaxFileCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxFileCountAttribute"/> class.
    /// </summary>
    /// <param name="maxFileCount">Maximum number of files.</param>
    public MaxFileCountAttribute(int maxFileCount)
    {
        MaxFileCount = maxFileCount;
    }

    /// <summary>
    /// Validate that the number of files in the collection is equal or smaller than the specified maximum or that it is null.
    /// </summary>
    /// <param name="value">Object to validate.</param>
    /// <param name="validationContext">Context in which a validation check is performed.</param>
    /// <returns>True if the number of files is equal or smaller than <see cref="MaxFileCount"/> or if it is null.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFileCollection formFileCollection)
        {
            var fileCount = formFileCollection.Count;

            if (fileCount > MaxFileCount)
                return new ValidationResult($"Too many files selected. Limit: {MaxFileCount}, Selected: {fileCount}.");

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
