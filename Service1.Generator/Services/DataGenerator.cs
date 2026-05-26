using service1.Models;

namespace service1.Services
{
    public class DataGenerator
    {
        // Random — класс для генерации случайных чисел
        // static — один экземпляр на весь класс, не создаём новый каждый раз
        private static readonly Random _random = new();

        // символы из которых будет состоять Payload
        private const string PayloadChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

        // главный метод — генерирует один батч
        public Batch GenerateBatch(int minItems, int maxItems)
        {
            // случайное количество объектов в батче
            int count = _random.Next(minItems, maxItems + 1);
            // maxItems + 1 потому что Next(min, max) — max не включается
            // то есть Next(10, 21) даст число от 10 до 20 включительно

            var items = new List<DataItem>();

            for (int i = 0; i < count; i++)
            {
                items.Add(GenerateItem());
            }

            return new Batch
            {
                BatchId = Guid.NewGuid(),          // генерируем уникальный ID
                GeneratedAt = DateTime.UtcNow,     // текущее время в UTC
                ItemsCount = items.Count,
                Items = items
            };
        }

        // генерирует один объект DataItem
        private DataItem GenerateItem()
        {
            int year = _random.Next(1701, 2341);
            // 2341 потому что Next не включает верхнюю границу
            // то есть Next(1701, 2341) даст год от 1701 до 2340

            // генерируем случайный месяц, день, час, минуту, секунду
            int month = _random.Next(1, 13);
            int day = _random.Next(1, DateTime.DaysInMonth(
                year < 1 ? 1 : year > 9999 ? 9999 : year, month) + 1);
            // DateTime.DaysInMonth — возвращает количество дней в месяце
            // нужно чтобы не сгенерировать 31 февраля например
            // ограничиваем год диапазоном DateTime (1-9999) для этого вычисления

            int hour = _random.Next(0, 24);
            int minute = _random.Next(0, 60);
            int second = _random.Next(0, 60);

            var dataValue = new DateTime(year, month, day, hour, minute, second);

            return new DataItem
            {
                Id = Guid.NewGuid(),
                Payload = GeneratePayload(),
                Value = _random.Next(1, 1000000),
                // 1000000 не включается, значит максимум 999999 — как в задании
                AdditionValue = GenerateAdditionValue(),
                YearValue = year,
                DataValue = dataValue
            };
        }

        // генерирует случайную строку длиной 32-64 символа
        private string GeneratePayload()
        {
            int length = _random.Next(32, 65);
            // 65 не включается — значит максимум 64

            // char[] — массив символов, потом соберём в строку
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                // берём случайный символ из PayloadChars
                chars[i] = PayloadChars[_random.Next(PayloadChars.Length)];
            }

            // new string(chars) — собирает массив символов в одну строку
            return new string(chars);
        }

        // генерирует дробное число с 10 знаками после запятой
        private decimal GenerateAdditionValue()
        {
            // генерируем случайное целое число и делим
            // получаем дробное в диапазоне 0.0000000000 - 9999.9999999999
            decimal value = (decimal)_random.Next(0, 100000) / 10000m;

            // Round — округляем до 10 знаков после запятой
            return Math.Round(value, 10);
        }
    }
}