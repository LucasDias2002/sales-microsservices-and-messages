using System;
using System.Collections.Generic;
using System.Text;

namespace Sales.MessageBus
{
    public interface IRabbitMQConsumer
    {
        public Task ConsumeAsync<T>(
            string exchange,
            string queue,
            string routingKey,
            Func<T, Task> handler);
    }
}
