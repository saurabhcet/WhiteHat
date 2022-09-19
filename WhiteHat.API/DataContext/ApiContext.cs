using WhiteHat.API.Model;
using Microsoft.EntityFrameworkCore;

namespace WhiteHat.API.DataContext
{
    public class ApiContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "MinutesDB");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Minute>()                 
                        .Property(e => e.Notes)                        
                        .HasConversion(
                            v => string.Join(',', v),
                            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());            
        }

        public DbSet<Minute> Minutes { get; set; }
    }
}