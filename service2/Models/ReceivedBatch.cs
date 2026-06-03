namespace service2.Models
{
    public class ReceivedBatch
    {
        public int Id { get; set; }
        public string SourceBatchId { get; set; } = string.Empty;
        public DateTime TransformedAt { get; set; }
        public int ItemsCount { get; set; }
        public int TokensUsed { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public string? ExcelFilePath { get; set; }
        public List<ReceivedItem> Items { get; set; } = new();
    }
}
