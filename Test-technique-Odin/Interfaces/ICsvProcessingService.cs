using Test_technique_Odin.Models;

namespace Test_technique_Odin.Interfaces
{
    public interface ICsvProcessingService
    {
        Task<IEnumerable<InputCsvModel>> ProcessCsvFile(IFormFile file, LoggerModel results);
    }
}
