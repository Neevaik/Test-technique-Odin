using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Test_technique_Odin.Controllers
{
    public class ImportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            
                string newFileName = GenerateFileName(file);

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", newFileName);

                CreateUploadDirectory(Path.GetDirectoryName(filePath));

                await SaveFile(file, filePath);

                ProceedFile(filePath);

                ViewBag.Message = "Fichier importé et traité avec succès";
            
            

            return View("Index");
        }

        private string GenerateFileName(IFormFile file)
        {
            string dateString = DateTime.Now.ToString("yyyyMMdd") + "-";
            return dateString + Path.GetFileName(file.FileName);
        }

        private async Task SaveFile(IFormFile file, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        private void CreateUploadDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void ProceedFile(string filePath)
        {
            string headers = "Date;TotalAmount;TotalBillingFee;Total3dsFee\r\n";
            System.IO.File.WriteAllText(filePath, headers, Encoding.UTF8);
        }
    }
}
