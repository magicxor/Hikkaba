using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the maximum file size of a IFormFile object.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class MaxFileSizeAttribute : ValidationAttribute
{
    /// <summary>
    /// Maximum file size in bytes.
    /// </summary>
    public long MaxFileSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxFileSizeAttribute"/> class.
    /// </summary>
    /// <param name="maxFileSize">Maximum file size in bytes.</param>
    public MaxFileSizeAttribute(long maxFileSize)
    {
        MaxFileSize = maxFileSize;
    }

    /// <summary>
    /// Validate that the file has a file size equal or smaller than the specified maximum (in bytes) or that it is null.
    /// </summary>
    /// <param name="value">Object to validate.</param>
    /// <param name="validationContext">Context in which a validation check is performed.</param>
    /// <returns>True if the file size is equal or smaller than <see cref="MaxFileSize"/> bytes or if it is null.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFile formFile)
        {
            if (formFile.Length > MaxFileSize)
                return new ValidationResult($"Maximum allowed file size: {MaxFileSize} bytes. Size of {formFile.FileName}: {formFile.Length}.");

            return ValidationResult.Success;
        }

        if (value is IFormFileCollection formFileCollection)
        {
            foreach (var collectionItem in formFileCollection)
            {
                if (collectionItem.Length > MaxFileSize)
                    return new ValidationResult($"Maximum allowed file size: {MaxFileSize} bytes. Size of {collectionItem.FileName}: {collectionItem.Length}.");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
