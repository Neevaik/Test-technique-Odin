
using Test_technique_Odin.Interfaces;

namespace Test_technique_Odin.Models.Services
{
    public class DataProcessingService : IDataProcessingService
    {
        public IEnumerable<ProcessedDataModel> ProcessInputData(IEnumerable<InputCsvModel> inputData)
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
    }

}
