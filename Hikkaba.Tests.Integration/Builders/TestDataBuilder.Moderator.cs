using Hikkaba.Data.Entities;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    /// <summary>
    ///     Assigns a user as moderator to a category.
    /// </summary>
    /// <param name="categoryAlias">The alias of the category.</param>
    /// <param name="moderatorUserName">The username of the user to assign as moderator.</param>
    public TestDataBuilder WithCategoryModerator(string categoryAlias, string moderatorUserName)
    {
        var category = GetCategory(categoryAlias);
        var moderator = GetUser(moderatorUserName);

        var categoryToModerator = new CategoryToModerator
        {
            Category = category,
            Moderator = moderator,
        };
        _dbContext.Set<CategoryToModerator>().Add(categoryToModerator);
        return this;
    }

    /// <summary>
    ///     Assigns multiple users as moderators to a category.
    /// </summary>
    /// <param name="categoryAlias">The alias of the category.</param>
    /// <param name="moderatorUserNames">The usernames of the users to assign as moderators.</param>
    public TestDataBuilder WithCategoryModerators(string categoryAlias, params string[] moderatorUserNames)
    {
        foreach (var userName in moderatorUserNames)
        {
            WithCategoryModerator(categoryAlias, userName);
        }

        return this;
    }
}
