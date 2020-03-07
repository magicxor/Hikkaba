using TPrimaryKey = System.Guid;
using Hikkaba.Common.Attributes;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Common.Extensions;
using Hikkaba.Data.Entities.Base.Current;
using Hikkaba.Data.Services;
using Hikkaba.Data.Entities.Attachments.Base;
using Hikkaba.Data.Extensions;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, TPrimaryKey>
    {
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IAuthenticatedUserService authenticatedUserService)
            : base(options)
        {
            _authenticatedUserService = authenticatedUserService;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // DateTimeKind
            var dateTimeOfKindValueConverterFactory = new DateTimeOfKindValueConverterFactory();
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
                {
                    var dateTimeKindAttribute = property.FieldInfo.GetCustomAttribute<DateTimeKindAttribute>();
                    if (dateTimeKindAttribute != null)
                    {
                        var dateTimeKind = dateTimeKindAttribute.Kind;
                        if (property.ClrType == typeof(DateTime))
                        {
                            property.SetValueConverter(dateTimeOfKindValueConverterFactory.GetDateTimeValueConverter(dateTimeKind));
                        }
                        else if (property.ClrType == typeof(DateTime?))
                        {
                            property.SetValueConverter(dateTimeOfKindValueConverterFactory.GetNullableDateTimeValueConverter(dateTimeKind));
                        }
                    }
                }
            }
            
            builder.Entity<Attachment>()
                .HasDiscriminator<int>("AttachmentType")
                .HasValue<Audio>(AttachmentTypes.Audio.ToInt())
                .HasValue<Document>(AttachmentTypes.Document.ToInt())
                .HasValue<Notice>(AttachmentTypes.Notice.ToInt())
                .HasValue<Picture>(AttachmentTypes.Picture.ToInt())
                .HasValue<Video>(AttachmentTypes.Video.ToInt());

            // many-to-many keys
            builder.Entity<CategoryToModerator>().HasKey(e => new { e.CategoryId, e.ApplicationUserId });

            // indices
            builder.Entity<Category>().HasIndex(e => e.Alias).IsUnique();
            builder.Entity<Category>().HasIndex(e => e.Name).IsUnique();
            
            builder.Entity<ApplicationUser>().HasIndex(e => e.UserName).IsUnique();
            builder.Entity<ApplicationUser>().HasIndex(e => e.NormalizedUserName).IsUnique();
            builder.Entity<ApplicationUser>().HasIndex(e => e.Email).IsUnique();
            builder.Entity<ApplicationUser>().HasIndex(e => e.NormalizedEmail).IsUnique();
            builder.Entity<ApplicationUser>().HasIndex(e => e.PhoneNumber).IsUnique();
            
            builder.Entity<ApplicationRole>().HasIndex(e => e.Name).IsUnique();
            builder.Entity<ApplicationRole>().HasIndex(e => e.NormalizedName).IsUnique();
        }

        private void OnSaveChangesAction()
        {
            ChangeTracker.DetectChanges();

            var authorId = _authenticatedUserService?.ApplicationUserClaims?.Id;

            if (authorId.HasValue)
            {
                {
                    var addedEntries = ChangeTracker.Entries().Where(x => x.State == EntityState.Added);
                    foreach (var addedEntry in addedEntries)
                    {
                        if (addedEntry.Entity is IBaseMutableEntity)
                        {
                            addedEntry.CurrentValues[nameof(IBaseMutableEntity.Created)] = DateTime.UtcNow;
                            addedEntry.Reference(nameof(IBaseMutableEntity.CreatedBy)).CurrentValue = this.GetLocalOrAttach<ApplicationUser>(authorId.Value);
                        }
                    } 
                }

                {
                    var modifiedEntries = ChangeTracker.Entries().Where(x => x.State == EntityState.Modified);
                    foreach (var modifiedEntry in modifiedEntries)
                    {
                        if (modifiedEntry.Entity is IBaseMutableEntity)
                        {
                            modifiedEntry.CurrentValues[nameof(IBaseMutableEntity.Modified)] = DateTime.UtcNow;
                            modifiedEntry.Reference(nameof(IBaseMutableEntity.ModifiedBy)).CurrentValue = this.GetLocalOrAttach<ApplicationUser>(authorId.Value);
                        }
                    }
                }
            }
        }
        
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnSaveChangesAction();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            OnSaveChangesAction();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            OnSaveChangesAction();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            OnSaveChangesAction();
            return base.SaveChanges();
        }
        
        public DbSet<Ban> Bans { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryToModerator> CategoriesToModerators { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<Audio> Audio { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<Video> Video { get; set; }
    }
}
