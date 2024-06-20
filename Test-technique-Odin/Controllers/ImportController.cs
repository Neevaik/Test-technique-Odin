
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
                var inputDate = await ProcessCsvFile(file);
                var processedData =  ProcessInputData(inputDate);
                await SaveProcessedData(processedData, file);



                ViewBag.Message = "File uploaded and processed successfully!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("File", $"Error processing file: {ex.Message}");
            }

            return View("Index");
        }



        private async Task<IEnumerable<InputCsvModel>> ProcessCsvFile(IFormFile file)
        {
            var inputData = new List<InputCsvModel>();

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

                    var data = new InputCsvModel
                    {
                        OrderId = columns[0].Trim('"'),
                        Nature = columns[1].Trim('"'),
                        OperationType = columns[2].Trim('"'),
                        Amount = decimal.Parse(columns[3].Trim('"'), CultureInfo.InvariantCulture),
                        Currency = columns[4].Trim('"'),
                        BillingFees = decimal.Parse(columns[5].Trim('"'), CultureInfo.InvariantCulture),
                        Date = DateTime.ParseExact(columns[6].Trim('"'), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        ChargebackDate = columns[7].Trim('"'),
                        TransferReference = columns[8].Trim('"'),
                        ExtraData = columns[9].Trim('"'),
                        SecureFee = decimal.Parse(columns[10].Trim('"'), CultureInfo.InvariantCulture)
                    };

                    inputData.Add(data);
                }
            }

            return inputData;
        }
        private IEnumerable<ProcessedDataModel> ProcessInputData(IEnumerable<InputCsvModel> inputData)
        {
            var processedData = new List<ProcessedDataModel>();
            var processedDates = new HashSet<DateTime>();

            foreach (var input in inputData)
            {
                if (!processedDates.Contains(input.Date))
                {
                    processedDates.Add(input.Date);

                    var processedItem = new ProcessedDataModel
                    {
                        Date = input.Date,
                        TotalAmount = 0,
                        TotalBillingFee = 0,
                        Total3dsFee = 0
                    };

                    processedData.Add(processedItem);
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
