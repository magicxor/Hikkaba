using Bogus;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Implementations;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Tests.Manual.Seed;

public sealed class Program
{
    private const int CustomSeed = 20157789;
    private static readonly Random Random = new(CustomSeed);
    private static readonly GuidGenerator GuidGenerator = new(CustomSeed);

    public static async Task Main(string[] args)
    {
        var customAppFactory = new CustomAppFactory("Server=(localdb)\\mssqllocaldb;Database=Hikkaba;Integrated Security=true;");
        using var scope = customAppFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            await dbContext.Database.MigrateAsync();
        }

        Randomizer.Seed = Random;
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // seed users
            logger.LogInformation("Seeding users...");

            var testUsers = new Faker<ApplicationUser>()
                .CustomInstantiator(f => new ApplicationUser
                {
                    UserName = f.Internet.UserName(),
                    NormalizedUserName = null,
                    Email = f.Internet.Email(),
                    NormalizedEmail = null,
                    EmailConfirmed = f.Random.Bool(),
                    PasswordHash = f.Internet.Password(8, 12),
                    SecurityStamp = null,
                    ConcurrencyStamp = null,
                    PhoneNumber = f.Phone.PhoneNumber(),
                    PhoneNumberConfirmed = f.Random.Bool(),
                    TwoFactorEnabled = f.Random.Bool(),
                    LockoutEnd = null,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    IsDeleted = f.Random.Bool(),
                    CreatedAt = f.Date.Past(),
                    LastLoginAt = f.Date.Past(),
                })
                .FinishWith((f, u) =>
                {
                    u.NormalizedUserName = u.UserName?.ToUpperInvariant();
                    u.NormalizedEmail = u.Email?.ToUpperInvariant();
                })
                .Generate(10);

            foreach (var testUser in testUsers)
            {
                await userMgr.CreateAsync(testUser, testUser.PasswordHash ?? string.Empty);
            }

            var categories = await dbContext.Categories
                .ToListAsync();

            foreach (var category in categories)
            {
                // seed threads
                logger.LogInformation("Seeding threads for category {CategoryAlias}...", category.Alias);

                var testThreads = new Faker<Thread>()
                    .CustomInstantiator(f => new Thread
                    {
                        IsDeleted = false,
                        CreatedAt = f.Date.Past(),
                        ModifiedAt = Random.Next(2) == 0 ? f.Date.Past() : null,
                        Title = f.Lorem.Sentence(5),
                        IsPinned = Random.Next(0, 3000) == 0,
                        IsClosed = Random.Next(0, 500) == 0,
                        IsCyclic = Random.Next(0, 3000) == 0,
                        BumpLimit = Random.Next(500, 1000),
                        Salt = GuidGenerator.GenerateSeededGuid(),
                        Category = category,
                    })
                    .Generate(150);

                dbContext.Threads.AddRange(testThreads);

                for (var threadIndex = 0; threadIndex < testThreads.Count; threadIndex++)
                {
                    var thread = testThreads[threadIndex];
                    logger.LogInformation("Seeding posts for thread {ThreadIndex}...", threadIndex);

                    var testPosts = new Faker<Post>()
                        .CustomInstantiator(f => new Post
                        {
                            BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                            IsDeleted = Random.Next(0, 50) == 0,
                            CreatedAt = f.Date.Past(),
                            ModifiedAt = Random.Next(2) == 0 ? f.Date.Past() : null,
                            IsSageEnabled = Random.Next(0, 5) == 0,
                            MessageText = string.Empty,
                            MessageHtml = f.Lorem.Paragraph(1)
                                          + (Random.Next(3) == 0 ? $"<b>{f.Lorem.Paragraph(1)}</b>" : f.Lorem.Paragraph(1))
                                          + "\r\n"
                                          + (Random.Next(3) == 0 ? $"<i>{f.Lorem.Paragraph(1)}</i>" : f.Lorem.Paragraph(1))
                                          + "\r\n"
                                          + (Random.Next(3) == 0 ? $"<u>{f.Lorem.Paragraph(1)}</u>" : f.Lorem.Paragraph(1)),
                            UserIpAddress = Random.Next(2) == 0 ? f.Internet.IpAddress().GetAddressBytes() : f.Internet.Ipv6Address().GetAddressBytes(),
                            UserAgent = f.Internet.UserAgent(),
                            ThreadLocalUserHash = [],
                            Thread = thread,
                        })
                        .FinishWith((f, p) =>
                        {
                            p.MessageText = HtmlUtilities.ConvertToPlainText(p.MessageHtml);
                            p.ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, p.UserIpAddress ?? []);
                        })
                        .Generate(150);

                    dbContext.Posts.AddRange(testPosts);

                    // add some replies
                    for (var replyIndex = 0; replyIndex < testPosts.Count; replyIndex++)
                    {
                        var post = testPosts[replyIndex];
                        if (Random.Next(0, 10) == 0)
                        {
                            // generate random amount of replies
                            // note that reply's date must be greater than the post's date
                            var repliesCount = Random.Next(1, 5);
                            var replies = new Faker<Post>()
                                .CustomInstantiator(f => new Post
                                {
                                    BlobContainerId = GuidGenerator.GenerateSeededGuid(),
                                    IsDeleted = Random.Next(0, 50) == 0,
                                    CreatedAt = f.Date.Between(post.CreatedAt.AddSeconds(1), DateTime.UtcNow),
                                    ModifiedAt = Random.Next(2) == 0 ? f.Date.Past() : null,
                                    IsSageEnabled = Random.Next(0, 5) == 0,
                                    MessageText = string.Empty,
                                    MessageHtml = f.Lorem.Paragraph(1)
                                                  + (Random.Next(3) == 0 ? $"<b>{f.Lorem.Paragraph(1)}</b>" : f.Lorem.Paragraph(1))
                                                  + "\r\n"
                                                  + (Random.Next(3) == 0 ? $"<i>{f.Lorem.Paragraph(1)}</i>" : f.Lorem.Paragraph(1)),
                                    UserIpAddress = Random.Next(2) == 0 ? f.Internet.IpAddress().GetAddressBytes() : f.Internet.Ipv6Address().GetAddressBytes(),
                                    UserAgent = f.Internet.UserAgent(),
                                    ThreadLocalUserHash = [],
                                    Thread = thread,
                                    MentionedPosts = new List<Post> { post },
                                })
                                .FinishWith((f, r) =>
                                {
                                    r.MessageText = HtmlUtilities.ConvertToPlainText(r.MessageHtml);
                                    r.ThreadLocalUserHash = hashService.GetHashBytes(thread.Salt, r.UserIpAddress ?? []);
                                })
                                .Generate(repliesCount);

                            dbContext.Posts.AddRange(replies);
                        }
                    }
                }
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation("Database seeded successfully");
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "An error occurred while seeding the database");
            throw;
        }
    }
}
