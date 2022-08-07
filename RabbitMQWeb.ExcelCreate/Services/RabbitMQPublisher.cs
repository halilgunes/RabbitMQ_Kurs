using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Services
{
    public class RabbitMQPublisher
    {
        private RabbitMQClientService rabbitMQClientService;
        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            this.rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(CreateExcelMessage createExcelMessage)
        {
            var channel = rabbitMQClientService.Connect();
            var serializedObject = JsonSerializer.Serialize(createExcelMessage);
            var messgeBody = Encoding.UTF8.GetBytes(serializedObject);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(RabbitMQClientService.ExchangeName, RabbitMQClientService.RoutingKey, true, properties, messgeBody);
        }
    }
}
