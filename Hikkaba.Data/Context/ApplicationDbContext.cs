using Hikkaba.Common.Enums;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hikkaba.Common.Extensions;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Data.Extensions;
using Hikkaba.Data.Utils;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Data.Context;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ContextConfigurationUtils.SetValueConverters(builder);
        builder.AddEfFunctions();

        base.OnModelCreating(builder);

        builder.Entity<Attachment>()
            .HasDiscriminator<AttachmentType>(nameof(Attachment.AttachmentType))
            .HasValue<Audio>(AttachmentType.Audio)
            .HasValue<Document>(AttachmentType.Document)
            .HasValue<Notice>(AttachmentType.Notice)
            .HasValue<Picture>(AttachmentType.Picture)
            .HasValue<Video>(AttachmentType.Video);

        builder.Entity<Ban>()
            .Property(e => e.BannedIpAddress)
            .HasConversion<byte[]>();

        builder.Entity<Ban>()
            .Property(e => e.BannedCidrLowerIpAddress)
            .HasConversion<byte[]>();

        builder.Entity<Ban>()
            .Property(e => e.BannedCidrUpperIpAddress)
            .HasConversion<byte[]>();

        builder.Entity<Post>()
            .Property(e => e.UserIpAddress)
            .HasConversion<byte[]>();

        builder.Entity<CategoryToModerator>()
            .HasOne(e => e.Category)
            .WithMany(e => e.Moderators)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CategoryToModerator>()
            .HasOne(e => e.Moderator)
            .WithMany(e => e.ModerationCategories)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Thread>()
            .HasOne(e => e.Category)
            .WithMany(e => e.Threads)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Post>()
            .HasOne(e => e.Thread)
            .WithMany(e => e.Posts)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Attachment>()
            .HasOne(e => e.Post)
            .WithMany(e => e.Attachments)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PostToReply>()
            .HasOne(e => e.Post)
            .WithMany(e => e.Replies)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PostToReply>()
            .HasOne(e => e.Reply)
            .WithMany(e => e.ParentPosts)
            .OnDelete(DeleteBehavior.Restrict);

        // indices
        builder.Entity<Category>().HasIndex(e => e.Alias).IsUnique();
        builder.Entity<Category>().HasIndex(e => e.Name).IsUnique();
    }

    public DbSet<Ban> Bans { get; set; } = null!;
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<CategoryToModerator> CategoriesToModerators { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Thread> Threads { get; set; } = null!;
    public DbSet<Audio> Audio { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Notice> Notices { get; set; } = null!;
    public DbSet<Picture> Pictures { get; set; } = null!;
    public DbSet<Video> Video { get; set; } = null!;
}
