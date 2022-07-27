using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Linq;

namespace RabbitMQ.Supplier
{
    class Program
    {
        static void Main(string[] args)
        {
            //amqps://gbedkufx:brgzymxtqJnJ_sBHcKToCIp8XpPSCiPS@chimpanzee.rmq.cloudamqp.com/gbedkufx
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(@"amqps://gbedkufx:brgzymxtqJnJ_sBHcKToCIp8XpPSCiPS@chimpanzee.rmq.cloudamqp.com/gbedkufx");
            using IConnection conn = factory.CreateConnection();

            var channel = conn.CreateModel();
            channel.ExchangeDeclare("fanout_exchange",ExchangeType.Fanout);
            channel.QueueDeclare("halil_queue", durable: true, false, false, null);
            channel.QueueBind("halil_queue", "fanout_exchange", "", null);
            

            Enumerable.Range(1, 40).ToList().ForEach(n => {
                string message = JsonConvert.SerializeObject($"{n}. Hello world");
                channel.BasicPublish("fanout_exchange", "", null, System.Text.Encoding.UTF8.GetBytes(message));
            });
            

            Console.WriteLine("Mesaj gönderildi");
            Console.ReadLine();
        }
    }
}
