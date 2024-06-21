using Test_technique_Odin.Models;

namespace Test_technique_Odin.Interfaces
{
    public interface IDataProcessingService
    {
        IEnumerable<ProcessedDataModel> ProcessInputData(IEnumerable<InputCsvModel> inputData);
    }
}
