using System;
using System.IO;
using System.Linq;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Entities.Attachments;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Common.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            /*
             * todo: 
             * we want to automatically set DateTimeKind while fetching DateTime properties from database
             * (http://www.gitshah.com/2015/02/how-to-automatically-set-datetimekind.html)
             * but there is no ObjectMaterialized event yet in EF7 (.net core 1.1.0): https://github.com/aspnet/EntityFramework/issues/626
             * so we set DateTimeKind every time while mapping dto <-> entity
            */
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<CategoryToModerator>().HasKey(e => new { e.CategoryId, e.ApplicationUserId });
            
            builder.Entity<Category>().HasIndex(e => e.Alias).IsUnique();
            builder.Entity<Category>().HasIndex(e => e.Name).IsUnique();
            builder.Entity<ApplicationUser>().HasIndex(e => e.Email).IsUnique();
            builder.Entity<ApplicationRole>().HasIndex(e => e.Name).IsUnique();

            //builder.Entity<Audio>().HasIndex(e => new { e.Size, e.Hash, e.Post }).IsUnique();
            //builder.Entity<Document>().HasIndex(e => new { e.Size, e.Hash, e.Post }).IsUnique();
            //builder.Entity<Picture>().HasIndex(e => new { e.Size, e.Hash, e.Post }).IsUnique();
            //builder.Entity<Video>().HasIndex(e => new { e.Size, e.Hash, e.Post }).IsUnique();
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
