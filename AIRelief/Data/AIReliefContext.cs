using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIRelief.Models
{
    public class AIReliefContext : IdentityDbContext
    {
        public AIReliefContext(DbContextOptions<AIReliefContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=aireliefdb.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-Group relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Group)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure User-UserStatistics relationship (one-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Statistics)
                .WithOne(s => s.User)
                .HasForeignKey<UserStatistics>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // Note: SaveChanges overrides below ensure Group defaults are applied before persisting.
   
        public DbSet<Question> Questions { get; set; }
        public DbSet<Group> Groups { get; set; }
        // Hide IdentityDbContext.Users member to use application User entity
        public new DbSet<User> Users { get; set; }
        public DbSet<UserStatistics> UserStatistics { get; set; }

        // No SaveChanges overrides here — PlanName is optional and should not be forced.
    }
}