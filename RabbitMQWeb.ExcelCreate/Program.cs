using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQWeb.ExcelCreate.Models;

namespace RabbitMQWeb.ExcelCreate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            //Burada aslýnda servisler için yeni bir scope yaratýyor iþi bitince kapatýyor.
            //Startup.cs içerisinde verilen servislere böylece eriþebiliyoruz ancak baþka bir scope yaratarak.
            using (var serv = host.Services.CreateScope())
            {
                var appDbContext = serv.ServiceProvider.GetRequiredService<AppDataContext>();
                var userManager = serv.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                appDbContext.Database.Migrate();

             if (!appDbContext.Users.Any())
                {
                    userManager.CreateAsync(new IdentityUser { Email = "deneme1@bla.com", UserName = "deneme1" }, "asd123").Wait();
                    userManager.CreateAsync(new IdentityUser { Email = "deneme2@bla.com", UserName = "deneme2" }, "asd123").Wait();
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
