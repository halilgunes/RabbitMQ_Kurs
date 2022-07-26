using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Consumers
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(@"amqps://gbedkufx:brgzymxtqJnJ_sBHcKToCIp8XpPSCiPS@chimpanzee.rmq.cloudamqp.com/gbedkufx");
            using IConnection conn = factory.CreateConnection();

            var channel = conn.CreateModel();
            channel.QueueBind("halil_queue", "fanout_exchange", "", null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Gelen mesaj : {message}");
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            };

            string consumerTag = channel.BasicConsume("halil_queue", false, consumer);
        }
    }
}
