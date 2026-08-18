using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Sales.MessageBus
{
    public class RabbitMQConsumer: IRabbitMQConsumer
    {
        public async Task ConsumeAsync<T>(
            string exchange,
            string queue,
            string routingKey,
            Func<T, Task> handler)
        {
            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange,
                ExchangeType.Topic);

            await channel.QueueDeclareAsync(
                queue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await channel.QueueBindAsync(
                queue,
                exchange,
                routingKey);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, args) =>
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                var message = JsonSerializer.Deserialize<T>(json);

                if (message != null)
                {
                    await handler(message);
                }

                await channel.BasicAckAsync(args.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(
                queue,
                autoAck: false,
                consumer: consumer);
        }
    }
}
