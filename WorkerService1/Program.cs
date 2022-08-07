using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileCreateWorkerService.Model;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FileCreateWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {

                    //config nesnesini hostContext'ten al�yor
                    IConfiguration Configuration = hostContext.Configuration;


                    services.AddDbContext<AdventureWorks2017Context>(options =>
                    {
                        options.UseSqlServer(Configuration.GetConnectionString("SQLServerAdventureWorks"));

                    });

                    //RabbitMQ'ya ba�lan�yoruz.
                    services.AddSingleton<ConnectionFactory>(p => new ConnectionFactory
                    {
                        Uri = new Uri(uriString: Configuration.GetConnectionString("RabbitMQ")),
                        DispatchConsumersAsync = true//asenkron �a�r�mda bunu yazmazsak consume etmez.
                    });

                    services.AddSingleton<RabbitMQClientService>();
                    services.AddHostedService<Worker>();
                    //services.AddHostedService<Worker1>();
                    //services.AddHostedService<Worker2>(); n tane ekleyebiliyoruz buradan worker servis.

                });
    }
}
