using Microsoft.EntityFrameworkCore;
using service2.Models;

namespace service2.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ReceivedBatch> Batches => Set<ReceivedBatch>();
        public DbSet<ReceivedItem> Items => Set<ReceivedItem>();
    }
}
