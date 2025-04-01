using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the maximum file size of a IFormFile object.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MaxFileSizeAttribute : ValidationAttribute
{
    /// <summary>
    /// Maximum file size in bytes.
    /// </summary>
    private readonly long _maxFileSize;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="maxFileSize">Maximum file size in bytes.</param>
    public MaxFileSizeAttribute(long maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    /// <summary>
    /// Validate that the file has a file size equal or smaller than the specified maximum (in bytes) or that it is null.
    /// </summary>
    /// <param name="value">Object to validate.</param>
    /// <param name="validationContext">Context in which a validation check is performed.</param>
    /// <returns>True if the file size is equal or smaller than <see cref="_maxFileSize"/> bytes or if it is null.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFile formFile)
        {
            if (formFile.Length > _maxFileSize)
                return new ValidationResult($"Maximum allowed file size: {_maxFileSize} bytes. Size of {formFile.FileName}: {formFile.Length}.");

            return ValidationResult.Success;
        }

        if (value is IFormFileCollection formFileCollection)
        {
            foreach (var collectionItem in formFileCollection)
            {
                if (collectionItem.Length > _maxFileSize)
                    return new ValidationResult($"Maximum allowed file size: {_maxFileSize} bytes. Size of {collectionItem.FileName}: {collectionItem.Length}.");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
