using System.Text.Json.Serialization;

namespace service2.DTOs
{
    public class Service3Response
    {
        // Политика SnakeCaseLower найдёт "envelope" и "items" автоматически
        public EnvelopeDto Envelope { get; set; } = new();
        public List<ItemDto> Items { get; set; } = new();
    }

    public class EnvelopeDto
    {
        // Явные атрибуты — они имеют приоритет над политикой SnakeCaseLower
        [JsonPropertyName("source_batch_id")]
        public string SourceBatchId { get; set; } = string.Empty;

        [JsonPropertyName("transformed_at")]
        public DateTime TransformedAt { get; set; }

        [JsonPropertyName("items_count")]
        public int ItemsCount { get; set; }

        [JsonPropertyName("tokens_used")]
        public int TokensUsed { get; set; }
    }

    public class ItemDto
    {
        // Атрибутов нет — SnakeCaseLower политика обработает эти имена корректно:
        // Uid → "uid", PayloadHash → "payload_hash", NumericValue → "numeric_value", и т.д.
        public string Uid { get; set; } = string.Empty;
        public string PayloadHash { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public long NumericValue { get; set; }
        public string PreciseValue { get; set; } = string.Empty;
        public string TimestampIso { get; set; } = string.Empty;
    }
}