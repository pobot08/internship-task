using System.Security.Cryptography;
using System.Text;
using Service3.Proxy.Models;

namespace Service3.Proxy.Services;

public class TransformationService : ITransformationService
{
    private static string CreateHash(string text)
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
            // timestamp_iso — DataValue с подставленным YearValue, формат ISO 8601 UTC, ровно 20 символов
            // Пример: "2185-07-14T03:22:11Z"
            var timestampIso = new DateTime(
                x.YearValue,
                x.DataValue.Month,
                x.DataValue.Day,
                x.DataValue.Hour,
                x.DataValue.Minute,
                x.DataValue.Second,
                DateTimeKind.Utc
            ).ToString("yyyy-MM-ddTHH:mm:ssZ");

            result.Add(new TransformedItem
            {
                Uid = x.Id.ToString(),
                Payload = x.Payload,
                PayloadHash = CreateHash(x.Payload),
                NumericValue = x.Value,
                PreciseValue = x.AdditionValue.ToString("F10"),   // ровно 10 знаков после запятой
                TimestampIso = timestampIso
            });
        }

        return result;
    }

    public int CountTokens(List<TransformedItem> items)
    {
        int count = 0;

        foreach (var item in items)
        {
            count += item.Uid.Length;           // Guid как строка: 36 символов
            count += item.PayloadHash.Length;   // "sha256:" + 64 hex = 71 символ
            count += item.Payload.Length;       // длина оригинального Payload
            count += item.NumericValue.ToString().Length;
            count += item.PreciseValue.Length;
            count += item.TimestampIso.Length;  // ровно 20 символов
        }

        return count;
    }
}