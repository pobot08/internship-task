using Service3.Proxy.Models;
namespace Service3.Proxy.Services;

public class Service1Client : IService1Client
{
    private readonly HttpClient _httpClient;

    public Service1Client(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Service1ItemDto>> GetLatestItemsAsync(int count)
    {
        var response = await _httpClient.GetAsync(
            $"/api/items/latest?count={count}");

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<List<Service1ItemDto>>();

        if (data == null)
            return new List<Service1ItemDto>();
        else return data;
    }
}

