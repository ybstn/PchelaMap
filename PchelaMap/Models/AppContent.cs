//using System;
//using Microsoft.EntityFrameworkCore;
//namespace PchelaMap.Models
//{
//    public class AppContent : DbContext
//    {
//        public DbSet<User> Users { get; set; }
//        public DbSet<UserWithTasks> UsersWithTasks { get; set; }
//    public AppContent()
//    {
//        Database.EnsureCreated();
//    }
//    protected override void OnConfiguring(DbContextOptionsBuilder optionsbuilder)
//    {
//        optionsbuilder.UseMySQL("server=mysql100.1gb.ru;UserId=gb_pcheladb;Password=ddca8eaawrt;database=gb_pcheladb;");
//    }
//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            modelBuilder.Entity<User>(entity =>
//            {
//                entity.HasKey(e => e.id);
//                entity.Property(e => e.CoordinateX).IsRequired();
//                entity.Property(e => e.CoordinateY).IsRequired();
//            });
//            modelBuilder.Entity<UserWithTasks>(entity =>
//            {
//                entity.HasKey(e => e.id);
//                entity.Property(e => e.CoordinateX).IsRequired();
//                entity.Property(e => e.CoordinateY).IsRequired();
//            });
//        }
//}
  
//}
