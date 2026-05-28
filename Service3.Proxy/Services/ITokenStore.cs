namespace Service3.Proxy.Services;

public interface ITokenStore
{
    int GetUsed(string key);

    bool TryAdd(string key, int limit, int amount);

    void Reset(string key);
}
