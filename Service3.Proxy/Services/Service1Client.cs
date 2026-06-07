using Service3.Proxy.Models;

namespace Service3.Proxy.Services;

public class Service1Client : IService1Client
{
    private readonly HttpClient _httpClient;

    public Service1Client(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // старый метод оставл€ем чтобы ничего не сломать
    public async Task<List<Service1ItemDto>> GetLatestItemsAsync(int count)
    {
        var response = await _httpClient.GetAsync($"/api/items/latest?count={count}");
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<LatestItemsResponse>();
        return data?.Items ?? [];
    }

    // новый метод Ч берЄт последний батч с его ID
    public async Task<(Guid BatchId, List<Service1ItemDto> Items)> GetLatestBatchAsync(int count)
    {
        // шаг 1 Ч получаем последний батч (его метаданные)
        var batchesResponse = await _httpClient.GetAsync("/api/batches?page=1&pageSize=1");
        batchesResponse.EnsureSuccessStatusCode();

        var batches = await batchesResponse.Content
            .ReadFromJsonAsync<BatchesListResponse>();

        var latestBatch = batches?.Items.FirstOrDefault();

        if (latestBatch == null)
            return (Guid.Empty, []);

        // шаг 2 Ч получаем items этого батча
        var itemsResponse = await _httpClient
            .GetAsync($"/api/batches/{latestBatch.BatchId}/items");
        itemsResponse.EnsureSuccessStatusCode();

        var items = await itemsResponse.Content
            .ReadFromJsonAsync<List<Service1ItemDto>>();

        return (latestBatch.BatchId, items ?? []);
    }
}