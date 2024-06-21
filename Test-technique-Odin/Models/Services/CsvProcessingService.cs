using System.Globalization;
using System.Text;
using Test_technique_Odin.Interfaces;

namespace Test_technique_Odin.Models.Services
{
    public class CsvProcessingService : ICsvProcessingService
    {
        public async Task<IEnumerable<InputCsvModel>> ProcessCsvFile(IFormFile file, LoggerModel results)
        {
            var inputData = new List<InputCsvModel>();

            using (var reader = new StreamReader(file.OpenReadStream(), Encoding.Default))
            {
                string line;
                int lineNumber = 1;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (lineNumber == 1)
                    {
                        lineNumber++;
                        continue; // Skip header line
                    }

                    try
                    {
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
                    catch (Exception ex)
                    {
                        results.ErrorLines.Add($"Line {lineNumber}: {ex.Message}");
                    }

                    lineNumber++;
                }
            }

            return inputData;
        }
    }

}
