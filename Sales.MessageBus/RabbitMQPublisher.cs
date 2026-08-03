using RabbitMQ.Client;
using System;

namespace Sales.MessageBus
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        public RabbitMQPublisher()
        {
        }

        public async Task Publish<T>(T message, string exchange, string routingKey)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Topic);

            var body = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));

            var properties = new BasicProperties();

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);
        }

        public async Task PublishInQueueAsync<T>(T message, string queueName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var body = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));

            var basicProperties = new BasicProperties();
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: basicProperties,
                body: body);
        }
    }
}

