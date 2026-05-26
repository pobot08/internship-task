namespace service1.Models
{
    public class DataItem
    {
        public Guid Id { get; set; }
        public string Payload { get; set; } = string.Empty;
        public int Value { get; set; }
        public decimal AdditionValue { get; set; }
        public int YearValue { get; set; }
        public DateTime DataValue { get; set; }
    }
}