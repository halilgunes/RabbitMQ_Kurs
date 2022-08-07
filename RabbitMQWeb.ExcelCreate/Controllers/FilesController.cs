using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using RabbitMQWeb.ExcelCreate.Models;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDataContext appDataContext;
        public FilesController(AppDataContext context)
        {
            appDataContext = context;
        }

        public async Task<IActionResult> Upload(IFormFile file, string userId, int fileId)
        {

            if (file.Length <= 0) return BadRequest();

            var userFile = await appDataContext.UserFiles.FirstOrDefaultAsync(f => f.Id == fileId);
            var filePath = file.FileName + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/files", filePath);

            using FileStream stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);
            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Created;

            await appDataContext.SaveChangesAsync();
            //SignalIR nofification kullanılacak burada.
            return Ok();
        }
    }
}