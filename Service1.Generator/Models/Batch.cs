namespace service1.Models
{
    public class Batch
    {
        public Guid BatchId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int ItemsCount { get; set; }
        public List<DataItem> Items { get; set; } = new();
    }
}