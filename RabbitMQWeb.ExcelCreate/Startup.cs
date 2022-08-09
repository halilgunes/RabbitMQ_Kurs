using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQWeb.ExcelCreate.Hubs;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;

namespace RabbitMQWeb.ExcelCreate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ConnectionFactory>(p => new ConnectionFactory
            {
                Uri = new Uri(uriString: Configuration.GetConnectionString("RabbitMQ")),
                DispatchConsumersAsync = true//asenkron �a�r�mda bunu yazmazsak consume etmez.
            });

            services.AddSingleton<RabbitMQClientService>();
            services.AddSingleton<RabbitMQPublisher>();
            services.AddDbContext<AppDataContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlServerRabbitMQ"));
            });


            //Identiy bilgileri dbde tutulacak,kullan�c�n�n email adresi unique olacak
            services.AddIdentity<IdentityUser, IdentityRole>(opt =>
             {
                 opt.Password.RequireUppercase = false;

                 opt.Password.RequireNonAlphanumeric = false;

                 opt.Password.RequiredLength = 4;

                 opt.User.RequireUniqueEmail = true;

             }).AddEntityFrameworkStores<AppDataContext>().AddDefaultTokenProviders();


            services.AddControllersWithViews();
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();//authentice olmal� kullan�c� diyoruz bu sat�rla.
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                //bu endpoint �zerinden haberle�me olacak.
                endpoints.MapHub<MyHub>("/MyHub");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
