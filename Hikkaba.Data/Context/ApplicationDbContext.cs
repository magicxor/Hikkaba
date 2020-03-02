using Hikkaba.Common.Attributes;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, TPrimaryKey>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
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

            // many-to-many keys
            builder.Entity<CategoryToModerator>().HasKey(e => new { e.CategoryId, e.ApplicationUserId });

            // indices
            builder.Entity<Category>().HasIndex(e => e.Alias).IsUnique();
            builder.Entity<Category>().HasIndex(e => e.Name).IsUnique();
            builder.Entity<ApplicationUser>().HasIndex(e => e.Email).IsUnique();
            builder.Entity<ApplicationRole>().HasIndex(e => e.Name).IsUnique();
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
