using Integration.SystemB.Models;
using Microsoft.EntityFrameworkCore;

namespace Integration.SystemB.Data
{
    public class SystemBDbContext : DbContext
    {
        public SystemBDbContext(DbContextOptions<SystemBDbContext> options)
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Message>()
                .HasIndex(x => x.SourceId)
                .IsUnique();
        }
    }
}