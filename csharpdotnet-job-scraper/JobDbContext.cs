using Microsoft.EntityFrameworkCore;

namespace indeed_scraper
{
    public class JobDbContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost;Database=JobDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
        }
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>().Property(p => p.Origin).IsRequired();
        }
    }
}