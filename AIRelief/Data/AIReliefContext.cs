using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        public DbSet<Fix> Fix { get; set; }
        public DbSet<Question> Questions { get; set; }
    }
}