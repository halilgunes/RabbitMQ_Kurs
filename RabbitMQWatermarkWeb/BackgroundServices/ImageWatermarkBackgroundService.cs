using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWatermarkWeb.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWatermarkWeb.BackgroundServices
{
    public class ImageWatermarkBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService rabbitMQClientService;
        private readonly ILogger<ImageWatermarkBackgroundService> logger;
        private IModel channel;
        public ImageWatermarkBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkBackgroundService> logger)
        {
            this.logger = logger;
            this.rabbitMQClientService = rabbitMQClientService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            channel = rabbitMQClientService.Connect();
            channel.BasicQos(0, 1, false);//kuyruktan kaçar kaçar alacağımızı,ki burada birer birer,düzenler.
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += ConsumerReceived;
            channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);//Autoack false olmalı yoksa çalışmıyor. Çünkü biz channel.BasicAck ile ConsumerReceived içeirisnde belirtiyoruz ack işlemini.
            return Task.CompletedTask;

        }

        private Task ConsumerReceived(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/", productImageCreatedEvent.ImageName);
                var siteName = "www.winimum.com";

                using var img = Image.FromFile(path);
                using var graphic = Graphics.FromImage(img);
                var font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = graphic.MeasureString(siteName, font);
                var color = Color.FromArgb(128, 255, 255, 255);
                var brush = new SolidBrush(color);

                var imagePosiiton = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));
                graphic.DrawString(siteName, font, brush, imagePosiiton);

                channel.BasicAck(@event.DeliveryTag, false);
                img.Save("wwwroot/images/watermarked/" + productImageCreatedEvent.ImageName);
                logger.LogInformation($"{productImageCreatedEvent.ImageName} resmine watermark ekledi.");
            }
            catch (Exception ex)
            {
                logger.LogInformation("Watermark eklenemedi");
                logger.LogError("Watermark eklemede hata:" + ex.ToString());
            }
            return Task.CompletedTask;
        }
    }
}
