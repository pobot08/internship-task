namespace service2.Models
{
    public class ReceivedItem
    {
        public int Id { get; set; }
        public string Uid { get; set; } = string.Empty;
        public string PayloadHash { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public long NumericValue { get; set; }
        public string PreciseValue { get; set; } = string.Empty;
        public string TimestampIso { get; set; } = string.Empty;
        public int ReceivedBatchId { get; set; }
        public ReceivedBatch Batch { get; set; } = null!;
    }
}
