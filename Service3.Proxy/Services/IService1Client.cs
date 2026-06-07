using Service3.Proxy.Models;

namespace Service3.Proxy.Services;

public interface IService1Client
{
    Task<List<Service1ItemDto>> GetLatestItemsAsync(int count);
    Task<(Guid BatchId, List<Service1ItemDto> Items)> GetLatestBatchAsync(int count);
}