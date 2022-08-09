using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using RabbitMQWeb.ExcelCreate.Hubs;
using RabbitMQWeb.ExcelCreate.Models;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDataContext appDataContext;
        private readonly IHubContext<MyHub> hubContext;
        public FilesController(AppDataContext context,IHubContext<MyHub> hubContext)
        {
            appDataContext = context;
            this.hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int fileId)
        {

            if (file.Length <= 0) return BadRequest();

            var userFile = await appDataContext.UserFiles.FirstOrDefaultAsync(f => f.Id == fileId);
            var filePath = file.FileName;
            var path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\\files", filePath);

            using FileStream stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);
            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Created;

            await appDataContext.SaveChangesAsync();
            //SignalIR nofification kullanılacak burada.

            //sadece ilgili kullanıcıya mesaj gönderiyor. clienta.
            await hubContext.Clients.User(userFile.UserId).SendAsync("Completed File creation");
            return Ok();
        }
    }
}