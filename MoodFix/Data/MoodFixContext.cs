using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MoodFix.Models
{
    public class MoodFixContext : DbContext
    {
        public MoodFixContext (DbContextOptions<MoodFixContext> options)
            : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=moodfixdb.db");
        }

        public DbSet<MoodFix.Models.Fix> Fix { get; set; }
    }
}
