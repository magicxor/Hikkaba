using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, TPrimaryKey>
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
            
            builder.Entity<CategoryToModerator>().HasKey(e => new { e.CategoryId, e.ApplicationUserId });
            
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
