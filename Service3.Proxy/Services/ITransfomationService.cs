using Service3.Proxy.Models;

namespace Service3.Proxy.Services;

public interface ITransformationService
{
    List<TransformedItem> Transform(List<Service1ItemDto> items);
    int CountTokens(List<TransformedItem> items);
}

