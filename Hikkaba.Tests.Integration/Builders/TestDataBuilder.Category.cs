using System;
using System.Collections.Generic;
using System.Linq;
using Hikkaba.Data.Entities;
using Hikkaba.Shared.Constants;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly List<Category> _categories = [];

    public IReadOnlyList<Category> Categories => _categories;

    /// <summary>
    ///     Returns the last created category.
    /// </summary>
    public Category LastCategory =>
        _categories.LastOrDefault()
        ?? throw new InvalidOperationException("Category not created. Call WithCategory() or WithDefaultCategory() first.");

    public TestDataBuilder WithCategory(
        string alias,
        string name,
        bool isDeleted = false,
        bool isHidden = false,
        int defaultBumpLimit = 500,
        bool showThreadLocalUserHash = false)
    {
        EnsureAdminExists();

        var category = new Category
        {
            IsDeleted = isDeleted,
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
            ModifiedAt = null,
            Alias = alias,
            Name = name,
            IsHidden = isHidden,
            DefaultBumpLimit = defaultBumpLimit,
            ShowThreadLocalUserHash = showThreadLocalUserHash,
            ShowOs = false,
            ShowBrowser = false,
            ShowCountry = false,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
            CreatedBy = Admin,
        };
        _categories.Add(category);
        _dbContext.Categories.Add(category);
        return this;
    }

    /// <summary>
    ///     Creates a default category with alias "b" and name "Random".
    /// </summary>
    public TestDataBuilder WithDefaultCategory()
    {
        return WithCategory("b", "Random");
    }

    public Category GetCategory(string alias)
    {
        return _categories.Find(c => c.Alias == alias)
               ?? throw new InvalidOperationException($"Category with alias '{alias}' not found.");
    }

    private void EnsureCategoryExists()
    {
        if (_categories.Count == 0)
        {
            throw new InvalidOperationException("Category must be created first. Call WithCategory() or WithDefaultCategory().");
        }
    }
}
