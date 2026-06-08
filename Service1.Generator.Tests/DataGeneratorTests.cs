using Microsoft.VisualStudio.TestTools.UnitTesting;
using service1.Services;
using service1.Models;

namespace Service1.Generator.Tests
{
    [TestClass]
    public class DataGeneratorTests
    {
        private readonly DataGenerator _generator = new();

        // --- тесты на GenerateBatch ---

        [TestMethod]
        public void GenerateBatch_ItemsCount_IsWithinRange()
        {
            // arrange — подготовка
            int minItems = 10;
            int maxItems = 20;

            // act — действие
            var batch = _generator.GenerateBatch(minItems, maxItems);

            // assert — проверка
            Assert.IsTrue(
                batch.Items.Count >= minItems && batch.Items.Count <= maxItems,
                $"Ожидалось от {minItems} до {maxItems} объектов, получили {batch.Items.Count}");
        }

        [TestMethod]
        public void GenerateBatch_BatchId_IsNotEmpty()
        {
            var batch = _generator.GenerateBatch(5, 10);
            Assert.AreNotEqual(Guid.Empty, batch.BatchId,
                "BatchId не должен быть пустым Guid");
        }

        [TestMethod]
        public void GenerateBatch_ItemsCount_MatchesActualCount()
        {
            var batch = _generator.GenerateBatch(10, 20);
            Assert.AreEqual(batch.Items.Count, batch.ItemsCount,
                "ItemsCount должен совпадать с реальным количеством объектов");
        }

        [TestMethod]
        public void GenerateBatch_GeneratedAt_IsUtc()
        {
            var batch = _generator.GenerateBatch(5, 10);
            Assert.AreEqual(DateTimeKind.Utc, batch.GeneratedAt.Kind,
                "GeneratedAt должен быть в UTC");
        }

        [TestMethod]
        public void GenerateBatch_AllItems_HaveUniqueIds()
        {
            var batch = _generator.GenerateBatch(10, 20);

            // собираем все ID в HashSet — он не допускает дубли
            var ids = new HashSet<Guid>(batch.Items.Select(i => i.Id));

            Assert.AreEqual(batch.Items.Count, ids.Count,
                "Все объекты должны иметь уникальные Id");
        }

        // --- тесты на DataItem ---

        [TestMethod]
        public void GenerateItem_Payload_LengthIsWithinRange()
        {
            // генерируем много батчей чтобы проверить разные случаи
            var batches = Enumerable.Range(0, 10)
                .Select(_ => _generator.GenerateBatch(10, 20))
                .ToList();

            foreach (var batch in batches)
            {
                foreach (var item in batch.Items)
                {
                    Assert.IsTrue(
                        item.Payload.Length >= 32 && item.Payload.Length <= 64,
                        $"Payload длиной {item.Payload.Length} вне диапазона 32-64");
                }
            }
        }

        [TestMethod]
        public void GenerateItem_Value_IsWithinRange()
        {
            var batch = _generator.GenerateBatch(10, 20);

            foreach (var item in batch.Items)
            {
                Assert.IsTrue(
                    item.Value >= 1 && item.Value <= 999999,
                    $"Value={item.Value} вне диапазона 1-999999");
            }
        }

        [TestMethod]
        public void GenerateItem_YearValue_IsWithinRange()
        {
            var batch = _generator.GenerateBatch(10, 20);

            foreach (var item in batch.Items)
            {
                Assert.IsTrue(
                    item.YearValue >= 1701 && item.YearValue <= 2340,
                    $"YearValue={item.YearValue} вне диапазона 1701-2340");
            }
        }

        [TestMethod]
        public void GenerateItem_DataValue_YearMatchesYearValue()
        {
            var batch = _generator.GenerateBatch(10, 20);

            foreach (var item in batch.Items)
            {
                Assert.AreEqual(
                    item.YearValue,
                    item.DataValue.Year,
                    $"Год DataValue ({item.DataValue.Year}) не совпадает с YearValue ({item.YearValue})");
            }
        }

        [TestMethod]
        public void GenerateItem_AdditionValue_HasTenDecimalPlaces()
        {
            var batch = _generator.GenerateBatch(10, 20);
            foreach (var item in batch.Items)
            {
                // ToString с инвариантной культурой — всегда точка как разделитель
                string valueStr = item.AdditionValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                int dotIndex = valueStr.IndexOf('.');
                int decimalPlaces = dotIndex == -1 ? 0 : valueStr.Length - dotIndex - 1;

                Assert.AreEqual(10, decimalPlaces,
                    $"AdditionValue должен иметь ровно 10 знаков после запятой, " +
                    $"получили {decimalPlaces}: {valueStr}");
            }
        }

        [TestMethod]
        public void GenerateItem_Payload_ContainsOnlyValidCharacters()
        {
            const string validChars =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

            var batch = _generator.GenerateBatch(10, 20);

            foreach (var item in batch.Items)
            {
                foreach (char c in item.Payload)
                {
                    Assert.IsTrue(
                        validChars.Contains(c),
                        $"Payload содержит недопустимый символ '{c}'");
                }
            }
        }

        // --- граничные случаи ---

        [TestMethod]
        public void GenerateBatch_WithMinEqualsMax_ReturnsExactCount()
        {
            var batch = _generator.GenerateBatch(5, 5);
            Assert.AreEqual(5, batch.Items.Count,
                "При minItems=maxItems=5 должно быть ровно 5 объектов");
        }

        [TestMethod]
        public void GenerateBatch_WithMinOne_DoesNotThrow()
        {
            // проверяем что не падает при минимальных значениях
            var batch = _generator.GenerateBatch(1, 1);
            Assert.AreEqual(1, batch.Items.Count);
        }
    }
}