using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PchelaMap.Areas.Identity.Data;
using Microsoft.Data.Sqlite;
namespace PchelaMap.Data
{
    public class ApplicationDbContext : IdentityDbContext<PchelaMapUser>
    {
        private const string passw = "bd"; 
        public DbSet<PchelaMapTask> Tasks { get; set; }
        public DbSet<PchelaMapUserTasks> UsersTasks { get; set; }
        public DbSet<UsersRefusedTasks> UsersRefusedFromTasks { get; set; }
        public DbSet<PromoBd> PromoCode { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // "DefaultConnection": "DataSource=app.db;Password=#Karta!Pchela0"
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string ConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = "app.db",
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = passw
            }.ToString();
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            optionsBuilder.UseSqlite(conn);

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PchelaMapTask>(entity =>
            {
                entity.Property(e => e.id).IsRequired();
            });
            base.OnModelCreating(builder);
            builder.Entity<PchelaMapUserTasks>().HasKey(o=>new {o.TaskID, o.UserID });
            builder.Entity<UsersRefusedTasks>().HasKey(o => new { o.TaskID, o.UserID });
            builder.Entity<PromoBd>().HasKey(o => new { o.code});
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
