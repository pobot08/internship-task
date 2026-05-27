namespace Service3.Proxy.Services;

public interface IService1Client 
{
    Task<List<Service1ItemDto>> GetLatestItemsAsync(int count);
}


