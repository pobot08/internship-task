namespace Service3.Proxy.Models;

public class EnvelopeResponse
{
    public Envelope Envelope { get; set; } = new();

    public List<TransformedItem> Items { get; set; } = [];
}

