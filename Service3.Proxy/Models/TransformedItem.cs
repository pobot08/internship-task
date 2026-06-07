using System.Text.Json.Serialization;

namespace Service3.Proxy.Models;

public class TransformedItem
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("payload_hash")]
    public string PayloadHash { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    [JsonPropertyName("numeric_value")]
    public int NumericValue { get; set; }

    [JsonPropertyName("precise_value")]
    public string PreciseValue { get; set; } = string.Empty;

    [JsonPropertyName("timestamp_iso")]
    public string TimestampIso { get; set; } = string.Empty;
}