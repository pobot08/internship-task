namespace Service3.Proxy.Models;

public class Envelope
{
    public string SourceBatchId { get; set; } = string.Empty;

    public DateTime TransformedAt { get; set; }

    public int ItemsCount { get; set; }

    public int TokensUsed { get; set; }

    public int TokensRemaining { get; set; }
}

