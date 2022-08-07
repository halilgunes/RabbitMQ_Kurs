using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileCreateWorkerService.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory connectionFactory;
        private IConnection connection;
        private IModel channel;
        public static readonly string QueueName = "queue-excel-file";
        private readonly ILogger<Worker> logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<Worker> logger)
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
            logger.LogInformation("WorkerService - RabbitMQ ile bağlantı kuruldu..");
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
