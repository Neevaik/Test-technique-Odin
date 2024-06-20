using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using Test_technique_Odin.Models;

namespace VotreNamespace.Controllers
{
    public class ImportController : Controller
    {
        private readonly ILogger<ImportController> _logger;

        public ImportController(ILogger<ImportController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {

            var results = new LoggerModel
            {
                ErrorLines = new List<string>()
            };

            try
            {
                var inputData = await ProcessCsvFile(file, results);
                results.LinePorcessed = inputData.Count();

                var processedData = ProcessInputData(inputData);
                results.SuccessfulLines = processedData.Count();

                await SaveProcessedData(processedData, file);

            }
            catch (Exception ex)
            {
                results.ErrorLines.Add($"Error processing file: {ex.Message}");
                foreach (var error in results.ErrorLines)
                {
                    _logger.LogError(error);
                }

                _logger.LogError(ex, "Error processing file");
            }

            _logger.LogInformation($"Total lines processed : {results.LinePorcessed}, Successful lines: {results.SuccessfulLines}");


            ViewData["Results"] = results;
            return View("Index");
        }

        private async Task<IEnumerable<InputCsvModel>> ProcessCsvFile(IFormFile file, LoggerModel results)
        {
            var inputData = new List<InputCsvModel>();

            using (var reader = new StreamReader(file.OpenReadStream(), Encoding.Default))
            {
                string line;
                bool isFirstLine = true;
                int lineNumber = 0;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }

                    try
                    {
                        var columns = line.Split(';');

                        var data = new InputCsvModel
                        {
                            OrderId = columns.Length > 0 ? columns[0].Trim('"') : throw new Exception("Missing OrderId"),
                            Nature = columns.Length > 1 ? columns[1].Trim('"') : throw new Exception("Missing Nature"),
                            OperationType = columns.Length > 2 ? columns[2].Trim('"') : throw new Exception("Missing OperationType"),
                            Amount = columns.Length > 3 ? decimal.Parse(columns[3].Trim('"'), CultureInfo.InvariantCulture) : throw new Exception("Missing Amount"),
                            Currency = columns.Length > 4 ? columns[4].Trim('"') : throw new Exception("Missing Currency"),
                            BillingFees = columns.Length > 5 ? decimal.Parse(columns[5].Trim('"'), CultureInfo.InvariantCulture) : throw new Exception("Missing BillingFees"),
                            Date = columns.Length > 6 ? DateTime.ParseExact(columns[6].Trim('"'), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : throw new Exception("Missing Date"),
                            ChargebackDate = columns.Length > 7 ? columns[7].Trim('"') : string.Empty,
                            TransferReference = columns.Length > 8 ? columns[8].Trim('"') : string.Empty,
                            ExtraData = columns.Length > 9 ? columns[9].Trim('"') : string.Empty,
                            SecureFee = columns.Length > 10 ? decimal.Parse(columns[10].Trim('"'), CultureInfo.InvariantCulture) : throw new Exception("Missing SecureFee")
                        };

                        inputData.Add(data);
                    }
                    catch (Exception ex)
                    {
                        results.ErrorLines.Add($"Line {lineNumber}: {ex.Message}");
                    }
                }
            }

            return inputData;
        }

        private IEnumerable<ProcessedDataModel> ProcessInputData(IEnumerable<InputCsvModel> inputData)
        {
            return inputData
                .Where(i => i.OperationType == "payment")
                .GroupBy(i => i.Date.Date)
                .Select(g => new ProcessedDataModel
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(d => d.Amount),
                    TotalBillingFee = g.Sum(d => d.BillingFees),
                    Total3dsFee = g.Sum(d => d.SecureFee)
                });
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



        #region Tools
        private string GenerateFileName(IFormFile file)
        {
            var getFileName = Path.GetFileName(file.FileName);
            string dateString = DateTime.Now.ToString("yyyyMMdd") + "-";

            return $"{dateString}ProcessedFile_{Path.GetFileNameWithoutExtension(getFileName)}.csv";
        }

        private void VerifyInputData(IEnumerable<InputCsvModel> inputData)
        {
            foreach (var data in inputData)
            {
                _logger.LogInformation($"OrderId: {data.OrderId}, " +
                    $"Date: {data.Date}, " +
                    $"Amount: {data.Amount}," +
                    $"Currency: {data.Currency}," +
                    $"BillingFees: {data.BillingFees}," +
                    $"ChargebackDate: {data.ChargebackDate}," +
                    $"TransferReference: {data.TransferReference}," +
                    $"ExtraData: {data.ExtraData}," +
                    $"SecureFee: {data.SecureFee}");
            }
        }

        #endregion
    }
}
