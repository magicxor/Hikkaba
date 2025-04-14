using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the maximum total size of a file collection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class MaxFileCollectionSizeAttribute : ValidationAttribute
{
    /// <summary>
    /// Maximum total size in bytes.
    /// </summary>
    public long MaxFileCollectionSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxFileCollectionSizeAttribute"/> class.
    /// </summary>
    /// <param name="maxFileCollectionSize">Maximum total size in bytes.</param>
    public MaxFileCollectionSizeAttribute(long maxFileCollectionSize)
    {
        MaxFileCollectionSize = maxFileCollectionSize;
    }

    /// <summary>
    /// Validate that the total size of the files in the collection is equal or smaller than the specified maximum (in bytes) or that it is null.
    /// </summary>
    /// <param name="value">Object to validate.</param>
    /// <param name="validationContext">Context in which a validation check is performed.</param>
    /// <returns>True if the total size is equal or smaller than <see cref="MaxFileCollectionSize"/> bytes or if it is null.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFileCollection formFileCollection)
        {
            if (!formFileCollection.Any())
                return ValidationResult.Success;

            var totalSize = formFileCollection.Sum(formFile => formFile.Length);

            if (totalSize > MaxFileCollectionSize)
                return new ValidationResult($"Maximum allowed total file size: {MaxFileCollectionSize} bytes. Actual: {totalSize}.");

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
