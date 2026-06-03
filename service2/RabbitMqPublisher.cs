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

        public RabbitMqPublisher()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(DbQueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(ExcelQueueName, durable: true, exclusive: false, autoDelete: false);
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
