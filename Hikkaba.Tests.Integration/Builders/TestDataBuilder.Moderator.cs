using System;
using System.Collections.Generic;
using System.Linq;
using Hikkaba.Data.Entities;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly List<ApplicationUser> _moderators = [];

    public IReadOnlyList<ApplicationUser> Moderators => _moderators;

    /// <summary>
    ///     Returns the last created moderator.
    /// </summary>
    public ApplicationUser LastModerator =>
        _moderators.LastOrDefault()
        ?? throw new InvalidOperationException("Moderator not created. Call WithModerator() first.");

    /// <summary>
    ///     Creates a moderator user with the specified username.
    /// </summary>
    public TestDataBuilder WithModerator(string userName)
    {
        var moderator = new ApplicationUser
        {
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = $"{userName}@example.com",
            NormalizedEmail = $"{userName.ToUpperInvariant()}@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = _guidGenerator.GenerateSeededGuid().ToString(),
            ConcurrencyStamp = _guidGenerator.GenerateSeededGuid().ToString(),
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
        };
        _moderators.Add(moderator);
        _dbContext.Users.Add(moderator);
        return this;
    }

    /// <summary>
    ///     Gets a moderator by username.
    /// </summary>
    public ApplicationUser GetModerator(string userName)
    {
        return _moderators.Find(m => m.UserName == userName)
               ?? throw new InvalidOperationException($"Moderator with username '{userName}' not found.");
    }

    /// <summary>
    ///     Assigns a moderator to a category.
    /// </summary>
    /// <param name="categoryAlias">The alias of the category.</param>
    /// <param name="moderatorUserName">The username of the moderator.</param>
    public TestDataBuilder WithCategoryModerator(string categoryAlias, string moderatorUserName)
    {
        var category = GetCategory(categoryAlias);
        var moderator = GetModerator(moderatorUserName);

        var categoryToModerator = new CategoryToModerator
        {
            Category = category,
            Moderator = moderator,
        };
        _dbContext.Set<CategoryToModerator>().Add(categoryToModerator);
        return this;
    }

    /// <summary>
    ///     Assigns multiple moderators to a category.
    /// </summary>
    /// <param name="categoryAlias">The alias of the category.</param>
    /// <param name="moderatorUserNames">The usernames of the moderators.</param>
    public TestDataBuilder WithCategoryModerators(string categoryAlias, params string[] moderatorUserNames)
    {
        foreach (var userName in moderatorUserNames)
        {
            WithCategoryModerator(categoryAlias, userName);
        }

        return this;
    }
}
