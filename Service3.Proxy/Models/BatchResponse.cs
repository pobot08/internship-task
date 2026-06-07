namespace Service3.Proxy.Models;

public class BatchesListResponse
{
    public int Page { get; set; }
    public int TotalCount { get; set; }
    public List<BatchMeta> Items { get; set; } = [];
}

public class BatchMeta
{
    public Guid BatchId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public int ItemsCount { get; set; }
}