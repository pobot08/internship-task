namespace Service3.Proxy.Models;

public class Service1ItemDto
{
    public Guid Id { get; set; }

    public string Payload { get; set; } = string.Empty;

    public int Value { get; set; }

    public decimal AdditionValue { get; set; }

    public DateTime DataValue { get; set; }
}
