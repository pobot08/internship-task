namespace service1.Models
{
    // этот класс хранит текущие настройки генератора
    // регистрируется как Singleton — живёт всё время работы сервиса
    // и BackgroundService и ConfigController работают с одним объектом
    public class RuntimeConfig
    {
        public int IntervalSeconds { get; set; } = 30;
        public int MinItems { get; set; } = 10;
        public int MaxItems { get; set; } = 20;
    }
}