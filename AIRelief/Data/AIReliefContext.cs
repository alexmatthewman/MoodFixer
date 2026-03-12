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

            // Configure UserQuestion relationships
            modelBuilder.Entity<UserQuestion>()
                .HasOne(uq => uq.User)
                .WithMany(u => u.UserQuestions)
                .HasForeignKey(uq => uq.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuestion>()
                .HasOne(uq => uq.Question)
                .WithMany()
                .HasForeignKey(uq => uq.QuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ActiveLesson – one active lesson per user at most
            modelBuilder.Entity<ActiveLesson>()
                .HasOne(al => al.User)
                .WithOne(u => u.ActiveLesson)
                .HasForeignKey<ActiveLesson>(al => al.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Index on User.TenantCode for tenant-scoped queries
            modelBuilder.Entity<User>()
                .HasIndex(u => u.TenantCode);

            // Index on Group.TenantCode for tenant-scoped queries
            modelBuilder.Entity<Group>()
                .HasIndex(g => g.TenantCode);

            // Unique index on Translation (Key + Language + Market)
            modelBuilder.Entity<Translation>(e =>
            {
                e.HasIndex(t => new { t.Key, t.Language, t.Market })
                 .IsUnique();
            });

            // Unique index on EmailTemplate (TemplateKey + Language + Market)
            modelBuilder.Entity<EmailTemplate>(e =>
            {
                e.HasIndex(t => new { t.TemplateKey, t.Language, t.Market })
                 .IsUnique();
            });
        }

        // Note: SaveChanges overrides below ensure Group defaults are applied before persisting.
   
        public DbSet<Question> Questions { get; set; }
        public DbSet<Group> Groups { get; set; }
        // Hide IdentityDbContext.Users member to use application User entity
        public new DbSet<User> Users { get; set; }
        public DbSet<UserStatistics> UserStatistics { get; set; }
        public DbSet<UserQuestion> UserQuestions { get; set; }
        public DbSet<ActiveLesson> ActiveLessons { get; set; }
        public DbSet<Translation> Translations { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        // No SaveChanges overrides here — PlanName is optional and should not be forced.
    }
}