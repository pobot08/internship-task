namespace Service3.Proxy.Models
{
    public class LatestItemsResponse
    {
        public int Count { get; set; }

        public List<Service1ItemDto> Items { get; set; } = [];
    }
}
