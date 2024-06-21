using Microsoft.AspNetCore.Mvc;
using Test_technique_Odin.Interfaces;
using Test_technique_Odin.Models;

namespace Test_technique_Odin.Controllers
{
    public class ImportController : Controller
    {
        private readonly ICsvProcessingService _csvService;
        private readonly IDataProcessingService _dataService;
        private readonly ILoggerService _logger;

        public ImportController(ICsvProcessingService csvService, IDataProcessingService dataService, ILoggerService logger)
        {
            _csvService = csvService;
            _dataService = dataService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var results = new LoggerModel();
            results.ErrorLines = new List<string>();

            try
            {
                var inputData = await _csvService.ProcessCsvFile(file, results);
                results.LinePorcessed = inputData.Count();

                var processedData = _dataService.ProcessInputData(inputData);
                results.SuccessfulLines = processedData.Count();

                await SaveProcessedData(processedData, file);

                ViewBag.Message = "File uploaded and processed successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file");
                foreach (var error in results.ErrorLines)
                {
                    _logger.LogError(error);
                }
            }

            _logger.LogInformation($"Uploaded has success. Lines processed: {results.LinePorcessed}, Successful lines: {results.SuccessfulLines}");

            ViewData["Results"] = results;
            return View("Index");
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
