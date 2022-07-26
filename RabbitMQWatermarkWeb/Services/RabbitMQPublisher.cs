using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

namespace RabbitMQWatermarkWeb.Services
{
    public class RabbitMQPublisher
    {
        private RabbitMQClientService rabbitMQClientService;
        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            this.rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(ProductImageCreatedEvent productCreatedEvent)
        {
            var channel = rabbitMQClientService.Connect();
            var serializedObject = JsonSerializer.Serialize(productCreatedEvent);
            var messgeBody = Encoding.UTF8.GetBytes(serializedObject);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(RabbitMQClientService.ExchangeName, RabbitMQClientService.RoutingKey,true, properties, messgeBody);
        }
    }
}
