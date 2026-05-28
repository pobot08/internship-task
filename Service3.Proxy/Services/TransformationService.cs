using System.Security.Cryptography;
using System.Text;
using Service3.Proxy.Models;

namespace Service3.Proxy.Services;

public class TransformationService : ITransformationService
{


    private string CreateHash(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] hash = SHA256.HashData(bytes);
        return "sha256:" + Convert.ToHexString(hash).ToLower();
    }


    public List<TransformedItem> Transform(List<Service1ItemDto> items)
    {
        var result = new List<TransformedItem>();

        foreach (var x in items)
        {
            var transformed = new TransformedItem
            {
                Uid = x.Id.ToString(),

                Payload = x.Payload,

                PayloadHash = CreateHash(x.Payload),

                NumericValue = x.Value,

                PreciseValue = x.AdditionValue.ToString(),

                TimestampIso = new DateTime(
                    x.YearValue,
                    x.DataValue.Month,
                    x.DataValue.Day,
                    x.DataValue.Hour,
                    x.DataValue.Minute,
                    x.DataValue.Second,
                    DateTimeKind.Utc
                ).ToString("O")
            };

            result.Add(transformed);
        }

        return result;
    }

    public int CountTokens(List<TransformedItem> items)
    {
        int count = 0;

        foreach (var item in items)
        {
            count += item.Uid.Length;
            count += item.Payload.Length;
            count += item.PayloadHash.Length;
            count += item.NumericValue.ToString().Length;
            count += item.PreciseValue.Length;
            count += item.TimestampIso.Length;
        }
        return ñount;
    }
}
