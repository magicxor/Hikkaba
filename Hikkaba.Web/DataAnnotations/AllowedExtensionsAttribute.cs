using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Hikkaba.Web.DataAnnotations;

/// <summary>
/// Validation attribute to check the extension of a file.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class AllowedExtensionsAttribute : ValidationAttribute
{
    /// <summary>
    /// Allowed file extensions.
    /// </summary>
    private readonly string[] _allowedExtensions;

    public string AllowedExtensions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedExtensionsAttribute"/> class.
    /// </summary>
    /// <param name="allowedExtensions">Allowed file extensions.</param>
    public AllowedExtensionsAttribute(string allowedExtensions)
    {
        AllowedExtensions = allowedExtensions;
        _allowedExtensions = allowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Validate that the file has an allowed extension or that it is null.
    /// </summary>
    /// <param name="value">Object to validate.</param>
    /// <param name="validationContext">Context in which a validation check is performed.</param>
    /// <returns>True if the file has an allowed extension or if it is null.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFile formFile)
        {
            if (!_allowedExtensions.Any(x => formFile.FileName.EndsWith("." + x, StringComparison.OrdinalIgnoreCase)))
                return new ValidationResult($"File extension is not allowed (file: {formFile.FileName}). Allowed extensions: {string.Join(", ", _allowedExtensions)}.");

            return ValidationResult.Success;
        }

        if (value is IFormFileCollection formFileCollection)
        {
            foreach (var collectionItem in formFileCollection)
            {
                if (!_allowedExtensions.Any(x => collectionItem.FileName.EndsWith("." + x, StringComparison.OrdinalIgnoreCase)))
                    return new ValidationResult($"File extension is not allowed (file: {collectionItem.FileName}). Allowed extensions: {string.Join(", ", _allowedExtensions)}.");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult($"Unknown type.");
    }
}
