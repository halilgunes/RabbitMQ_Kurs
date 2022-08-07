using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;
using Shared;
namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {

        private readonly AppDataContext appDataContext;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RabbitMQPublisher rabbitMQPublisher;

        public ProductController(AppDataContext appDataContext, UserManager<IdentityUser> userManager, RabbitMQPublisher rabbitMQPublisher)
        {
            this.appDataContext = appDataContext;
            this.userManager = userManager;
            this.rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var fileName = $"product-excel-{User.Identity.Name}-{Guid.NewGuid().ToString().Substring(1, 10)}";

            UserFile userFile = new UserFile
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };

            await appDataContext.UserFiles.AddAsync(userFile);
            await appDataContext.SaveChangesAsync();

            //EntityFramework entity'i savechanges ile kaydedince boş olan alanları da dbden çekip scopetaki entitynin alanlarını dolduruyor. 
            //Burada UserFile nesnesinin FileId'sini bu şekilde set ediyor.
            var publishObject = new CreateExcelMessage { 
                FileId = userFile.Id, 
                UserId = userFile.UserId 
            };
            rabbitMQPublisher.Publish(publishObject);
            TempData["CreatingExcelFile"] = true;

            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            return View(await appDataContext.UserFiles.Where(f => f.UserId == user.Id).ToListAsync());
        }


    }
}