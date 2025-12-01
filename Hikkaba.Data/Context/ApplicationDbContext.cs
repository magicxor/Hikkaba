using System;
using Hikkaba.Shared.Enums;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Data.Extensions;
using Hikkaba.Data.Utils;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Data.Context;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, int>, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ContextConfigurationUtils.SetValueConverters(builder);
        builder.AddEfFunctions();

        base.OnModelCreating(builder);

        // CategoryToModerator M:M relationship
        // On ApplicationUser delete - use NoAction (users are rarely deleted, clean up manually)
        // On Category delete - cascade delete CategoryToModerator records
        builder.Entity<ApplicationUser>()
            .HasMany(p => p.ModerationCategories)
            .WithOne(d => d.Moderator)
            .HasForeignKey(d => d.ModeratorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Category>()
            .HasMany(p => p.Moderators)
            .WithOne(d => d.Category)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

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

        // RelatedPostId is stored as a regular column without FK constraint
        // to avoid multiple cascade paths (Category -> Thread -> Post and Category -> Ban)
        builder.Entity<Ban>()
            .Property(e => e.RelatedPostId)
            .HasColumnName("RelatedPostId");

        // On Category delete - cascade delete related bans (forgive all banned users in that category)
        builder.Entity<Ban>()
            .HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // On ApplicationUser delete - use NoAction (users are rarely deleted, clean up manually)
        builder.Entity<Ban>()
            .HasOne(e => e.CreatedBy)
            .WithMany(u => u.CreatedBans)
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Ban>()
            .HasOne(e => e.ModifiedBy)
            .WithMany(u => u.ModifiedBans)
            .HasForeignKey(e => e.ModifiedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Post>()
            .Property(e => e.UserIpAddress)
            .HasConversion<byte[]>();

        // On Category delete - cascade delete all Threads
        builder.Entity<Category>()
            .HasMany(p => p.Threads)
            .WithOne(d => d.Category)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category -> ApplicationUser relationships
        // On ApplicationUser delete - use NoAction (users are rarely deleted, clean up manually)
        builder.Entity<Category>()
            .HasOne(e => e.CreatedBy)
            .WithMany(u => u.CreatedCategories)
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Category>()
            .HasOne(e => e.ModifiedBy)
            .WithMany(u => u.ModifiedCategories)
            .HasForeignKey(e => e.ModifiedById)
            .OnDelete(DeleteBehavior.NoAction);

        // On Thread delete - cascade delete all Posts
        builder.Entity<Thread>()
            .HasMany(p => p.Posts)
            .WithOne(d => d.Thread)
            .HasForeignKey(d => d.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        // Thread -> ApplicationUser relationship
        // On ApplicationUser delete - use NoAction (users are rarely deleted, clean up manually)
        builder.Entity<Thread>()
            .HasOne(e => e.ModifiedBy)
            .WithMany()
            .HasForeignKey(e => e.ModifiedById)
            .OnDelete(DeleteBehavior.NoAction);

        // On Post delete - cascade delete all attachments
        // Use NoAction from Attachment side to avoid multiple cascade paths
        builder.Entity<Post>()
            .HasMany(p => p.Audios)
            .WithOne(d => d.Post)
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Post>()
            .HasMany(p => p.Documents)
            .WithOne(d => d.Post)
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Post>()
            .HasMany(p => p.Notices)
            .WithOne(d => d.Post)
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Post>()
            .HasMany(p => p.Pictures)
            .WithOne(d => d.Post)
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Post>()
            .HasMany(p => p.Videos)
            .WithOne(d => d.Post)
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Post -> ApplicationUser relationship
        // On ApplicationUser delete - use NoAction (users are rarely deleted, clean up manually)
        builder.Entity<Post>()
            .HasOne(e => e.ModifiedBy)
            .WithMany(u => u.ModifiedPosts)
            .HasForeignKey(e => e.ModifiedById)
            .OnDelete(DeleteBehavior.NoAction);

        // Notice -> ApplicationUser relationship
        // On ApplicationUser delete - use NoAction (users are rarely deleted, clean up manually)
        builder.Entity<Notice>()
            .HasOne(e => e.CreatedBy)
            .WithMany(u => u.CreatedNotices)
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.NoAction);

        // PostToReply M:M relationship
        // On Post delete - cascade delete PostToReply records,
        // but do NOT delete related posts (replies or mentioned posts)
        // Note: SQL Server doesn't allow multiple cascade paths, so we use ClientCascade for ReplyId
        // which requires EF Core to load related entities before deletion
        builder.Entity<Post>()
            .HasMany(e => e.Replies)
            .WithMany(e => e.MentionedPosts)
            .UsingEntity<PostToReply>(
                l => l.HasOne<Post>(nameof(PostToReply.Post)).WithMany(x => x.RepliesToThisMentionedPost).OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<Post>(nameof(PostToReply.Reply)).WithMany(x => x.MentionedPostsToThisReply).OnDelete(DeleteBehavior.ClientCascade));

        // indices
        builder.Entity<Category>().HasIndex(e => e.Alias).IsUnique();
        builder.Entity<Category>().HasIndex(e => e.Name).IsUnique();
        builder.Entity<Category>().HasIndex(e => e.IsDeleted);

        builder.Entity<Thread>().HasIndex(e => e.CreatedAt);
        builder.Entity<Thread>().HasIndex(e => e.IsPinned);
        builder.Entity<Thread>().HasIndex(e => e.IsDeleted);

        builder.Entity<Post>().HasIndex(e => e.BlobContainerId).IsUnique();
        builder.Entity<Post>().HasIndex(e => e.CreatedAt);
        builder.Entity<Post>().HasIndex(e => e.IsSageEnabled);
        builder.Entity<Post>().HasIndex(p => p.IsDeleted).IncludeProperties(p => p.ThreadId);

        builder.Entity<Attachment>().HasIndex(e => e.BlobId).IsUnique();

        builder.Entity<Ban>().HasIndex(e => e.EndsAt);
        builder.Entity<Ban>().HasIndex(e => e.BannedIpAddress);
        builder.Entity<Ban>().HasIndex(e => e.BannedCidrLowerIpAddress);
        builder.Entity<Ban>().HasIndex(e => e.BannedCidrUpperIpAddress);
        builder.Entity<Ban>().HasIndex(e => e.CountryIsoCode);
        builder.Entity<Ban>().HasIndex(e => e.IsDeleted);
        builder.Entity<Ban>().HasIndex(e => e.RelatedPostId).IsUnique();
    }

    public DbSet<Ban> Bans { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<CategoryToModerator> CategoriesToModerators { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<PostToReply> PostsToReplies { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<Thread> Threads { get; set; } = null!;
    public DbSet<Audio> Audios { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Notice> Notices { get; set; } = null!;
    public DbSet<Picture> Pictures { get; set; } = null!;
    public DbSet<Video> Videos { get; set; } = null!;
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}
