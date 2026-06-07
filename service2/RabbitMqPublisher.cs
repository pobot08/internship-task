using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using System.Text;

namespace service2
{
    public class RabbitMqPublisher
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;

        public const string DbQueueName = "batch-db";
        public const string ExcelQueueName = "batch-excel";

        public RabbitMqPublisher(IConfiguration config)
        {
            var host = config["RabbitMQ:Host"] ?? "localhost";

            var factory = new ConnectionFactory { HostName = host };

            // Пробуем подключиться несколько раз
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(DbQueueName, durable: true, exclusive: false, autoDelete: false);
                    _channel.QueueDeclare(ExcelQueueName, durable: true, exclusive: false, autoDelete: false);
                    return; // успешно подключились
                }
                catch
                {
                    Console.WriteLine($"RabbitMQ недоступен, попытка {i + 1}/10...");
                    Thread.Sleep(3000); // ждём 3 секунды
                }
            }

            throw new Exception("Не удалось подключиться к RabbitMQ");
        }

        public void Publish(string data)
        {
            var body = Encoding.UTF8.GetBytes(data);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish("", DbQueueName, properties, body);
            _channel.BasicPublish("", ExcelQueueName, properties, body);
        }
    }
}
