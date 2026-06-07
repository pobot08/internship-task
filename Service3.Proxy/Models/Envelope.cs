using System.Text.Json.Serialization;

public class Envelope
{
    [JsonPropertyName("source_batch_id")]
    public Guid SourceBatchId { get; set; }

    [JsonPropertyName("transformed_at")]
    public DateTime TransformedAt { get; set; }

    [JsonPropertyName("items_count")]
    public int ItemsCount { get; set; }

    [JsonPropertyName("tokens_used")]
    public int TokensUsed { get; set; }

    [JsonPropertyName("tokens_remaining")]
    public int TokensRemaining { get; set; }
}