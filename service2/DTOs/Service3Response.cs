namespace service2.DTOs
{
    public class Service3Response
    {
        public EnvelopeDto Envelope { get; set; } = new();
        public List<ItemDto> Items { get; set; } = new();
    }

    public class EnvelopeDto
    {
        public string SourceBatchId { get; set; } = string.Empty;
        public DateTime TransformedAt { get; set; }
        public int ItemsCount { get; set; }
        public int TokensUsed { get; set; }
    }

    public class ItemDto
    {
        public string Uid { get; set; } = string.Empty;
        public string PayloadHash { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public long NumericValue { get; set; }
        public string PreciseValue { get; set; } = string.Empty;
        public string TimestampIso { get; set; } = string.Empty;
    }
}
