using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory connectionFactory;
        private IConnection connection;
        private IModel channel;
        public static readonly string ExchangeName = "ExcelDirectExchange";
        public static readonly string RoutingKey = "excel-route-file";
        public static readonly string QueueName = "queue-excel-file";
        private readonly ILogger<RabbitMQClientService> logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
            Connect();
        }

        public IModel Connect()
        {
            connection = connectionFactory.CreateConnection();
            if (channel is { IsOpen: true })
            {
                return channel;
            }

            channel = connection.CreateModel();
            channel.ExchangeDeclare(ExchangeName, type: ExchangeType.Direct, true, false);
            channel.QueueDeclare(QueueName, true, false, false, null);
            channel.QueueBind(QueueName, ExchangeName, RoutingKey, null);
            logger.LogInformation("RabbitMQ ile bağlantı kuruldu..");
            return channel;
        }

        public void Dispose()
        {
            channel?.Close();
            channel?.Dispose();

            connection?.Close();
            connection?.Dispose();

            logger.LogInformation("RabbitMQ ile bağlantı koptu..");
        }
    }
}
