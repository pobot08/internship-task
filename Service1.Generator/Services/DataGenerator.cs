using service1.Models;

namespace service1.Services
{
    public class DataGenerator
    {
        private static readonly Random _random = new();

        // символы из которых будет состоять Payload
        private const string PayloadChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

        // главный метод — генерирует один батч
        public Batch GenerateBatch(int minItems, int maxItems)
        {
            int count = _random.Next(minItems, maxItems + 1);

            var items = new List<DataItem>();

            for (int i = 0; i < count; i++)
            {
                items.Add(GenerateItem());
            }

            return new Batch
            {
                BatchId = Guid.NewGuid(),
                GeneratedAt = DateTime.UtcNow,
                ItemsCount = items.Count,
                Items = items
            };
        }

        // генерирует один объект DataItem
        private DataItem GenerateItem()
        {
            int year = _random.Next(1701, 2341);

            // генерируем случайный месяц, день, час, минуту, секунду
            int month = _random.Next(1, 13);
            int day = _random.Next(1, DateTime.DaysInMonth(
                year < 1 ? 1 : year > 9999 ? 9999 : year, month) + 1);

            int hour = _random.Next(0, 24);
            int minute = _random.Next(0, 60);
            int second = _random.Next(0, 60);

            var dataValue = new DateTime(year, month, day, hour, minute, second);

            return new DataItem
            {
                Id = Guid.NewGuid(),
                Payload = GeneratePayload(),
                Value = _random.Next(1, 1000000),
                AdditionValue = GenerateAdditionValue(),
                YearValue = year,
                DataValue = dataValue
            };
        }

        // генерирует случайную строку длиной 32-64 символа
        private string GeneratePayload()
        {
            int length = _random.Next(32, 65);

            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = PayloadChars[_random.Next(PayloadChars.Length)];
            }

            return new string(chars);
        }

        // генерирует дробное число с 10 знаками после запятой
        private decimal GenerateAdditionValue()
        {
            int integer = _random.Next(0, 10000);

            long first9 = (long)_random.NextInt64(0, 1_000_000_000L);

            int lastDigit = _random.Next(1, 10);

            long fractional = first9 * 10 + lastDigit;

            decimal value = integer + fractional / 10_000_000_000m;

            return value;
        }
    }
}