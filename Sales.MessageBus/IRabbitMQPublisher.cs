using System;
using System.Collections.Generic;
using System.Text;

namespace Sales.MessageBus
{
    public interface IRabbitMQPublisher
    {
        Task Publish<T>(T message, string exchange, string routingKey);
        Task PublishInQueueAsync<T>(T message, string queueName);
    }
}
