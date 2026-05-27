namespace Service3.Proxy.Models;

public class TransformedItem
{
    public Guid Uid { get; set; }

    public string PayloadHash { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public int NumericValue { get; set; }

    public string PreciseValue { get; set; } = string.Empty;

    public string TimestampIso { get; set; } = string.Empty;
}
