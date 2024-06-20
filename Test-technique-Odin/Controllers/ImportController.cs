
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using Test_technique_Odin.Models;

namespace VotreNamespace.Controllers
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
            try
            {
                var processedData = await ProcessCsvFile(file);

                await SaveProcessedData(processedData, file);



                ViewBag.Message = "File uploaded and processed successfully!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("File", $"Error processing file: {ex.Message}");
            }

            return View("Index");
        }

        private async Task<IEnumerable<ProcessedDataModel>> ProcessCsvFile(IFormFile file)
        {
            var processedData = new List<ProcessedDataModel>();
            var processedDates = new HashSet<DateTime>(); // Utilisation d'un HashSet pour suivre les dates déjà traitées

            using (var reader = new StreamReader(file.OpenReadStream(), Encoding.Default))
            {
                string line;
                bool isFirstLine = true; // Skip header line

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue; // Skip header line
                    }

                    var columns = line.Split(';');

                    var date = DateTime.ParseExact(columns[6].Trim('"'), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    // Vérifie si la date a déjà été traitée
                    if (!processedDates.Contains(date))
                    {
                        processedDates.Add(date); // Ajouter la date au HashSet pour marquer comme traitée

                        var model = new ProcessedDataModel
                        {
                            Date = date,
                            TotalAmount = 0,
                            TotalBillingFee = 0,
                            Total3dsFee = 0 
                        };

                        processedData.Add(model);
                    }
                }
            }

            return processedData;
        }

        private async Task SaveProcessedData(IEnumerable<ProcessedDataModel> data, IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string newFileName = GenerateFileName(file);

            var filePath = Path.Combine(uploadsFolder, newFileName);

            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteLineAsync("Date;TotalAmount;TotalBillingFee;Total3dsFee");

                foreach (var item in data)
                {
                    await writer.WriteLineAsync($"{item.Date.ToString("yyyy-MM-dd HH:mm:ss")};{item.TotalAmount};{item.TotalBillingFee};{item.Total3dsFee}");
                }
            }
        }


        private string GenerateFileName(IFormFile file)
        {
            var getFileName = Path.GetFileName(file.FileName);
            string dateString = DateTime.Now.ToString("yyyyMMdd") + "-";

            return $"{dateString}ProcessedFile_{Path.GetFileNameWithoutExtension(getFileName)}.csv"; ;
        }


    }
}
