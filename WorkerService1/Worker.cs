using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FileCreateWorkerService.Model;
using FileCreateWorkerService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private RabbitMQClientService rabbitMQClientService;
        private readonly IServiceProvider serviceProvider;
        private IModel channel;
        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            this.rabbitMQClientService = rabbitMQClientService;
            this.serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            channel = rabbitMQClientService.Connect();
            channel.BasicQos(0, 1, false);//kuyruktan birer birer al.
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                await Task.Delay(1000, stoppingToken);
            }

        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);
            var excel = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            using var ms = new MemoryStream();
            var workBook = new XLWorkbook();
            var ds = new DataSet();
            ds.Tables.Add(GetDataTable("Products"));
            workBook.Worksheets.Add(ds);
            workBook.SaveAs(ms);

            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()),"file",Guid.NewGuid().ToString()+".xlsx");
            var baseUrl = "http://localhost:3483/api/files";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?fileId={excel.FileId}", multipartFormDataContent);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File (Id : {excel.FileId}) was created successfully");
                    channel.BasicAck(@event.DeliveryTag, false);
                }
            }

        }

        /// <summary>
        /// Excele basýlacak datayý tutan DataTable'ý getirir.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected DataTable GetDataTable(string tableName)
        {
            List<Product> products;
            DataTable dt;
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2017Context>();
                products = context.Products.ToList();

                dt = new DataTable() { TableName = tableName };
                dt.Columns.Add("ProductId",typeof(int));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("ProductNumber", typeof(string));
                dt.Columns.Add("Color", typeof(string));

                products.ForEach(p =>
                {
                    dt.Rows.Add(p.ProductId, p.Name, p.ProductNumber, p.Color);
                });
            }
            return dt;
        }
    }
}
