using Microsoft.EntityFrameworkCore;
using service1.Models;

namespace service1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Batch> Batches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.HasKey(b => b.BatchId);

                entity.Property(b => b.Items)
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                          v => System.Text.Json.JsonSerializer.Deserialize<List<DataItem>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
                      );
            });
        }
    }
}