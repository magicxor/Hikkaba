using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Hikkaba.Shared.Extensions;
using Humanizer;
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
                return new ValidationResult($"File size exceeds the limit. Maximum: {MaxFileSize.Bytes().Humanize(CultureInfo.InvariantCulture)}. Size of \"{formFile.FileName.Cut(20)}\": {formFile.Length.Bytes().Humanize(CultureInfo.InvariantCulture)}.");

            return ValidationResult.Success;
        }

        if (value is IFormFileCollection formFileCollection)
        {
            foreach (var collectionItem in formFileCollection)
            {
                if (collectionItem.Length > MaxFileSize)
                    return new ValidationResult($"File size exceeds the limit. Maximum: {MaxFileSize.Bytes().Humanize(CultureInfo.InvariantCulture)}. Size of \"{collectionItem.FileName.Cut(20)}\": {collectionItem.Length.Bytes().Humanize(CultureInfo.InvariantCulture)}.");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult("Unknown type.");
    }
}
