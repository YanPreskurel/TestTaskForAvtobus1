using Microsoft.EntityFrameworkCore;
using UrlShortenerApp.Models;

namespace UrlShortenerApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UrlMapping> Urls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Индекс для мгновенного редиректа время - (O(1))
            modelBuilder.Entity<UrlMapping>()
                .HasIndex(u => u.ShortCode)
                .IsUnique();

            // Индекс для ускорения проверки наличия ссылки при создании
            modelBuilder.Entity<UrlMapping>()
                .HasIndex(u => u.LongUrl);
        }
    }
}