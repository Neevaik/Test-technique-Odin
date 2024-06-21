namespace Test_technique_Odin.Interfaces
{
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogError(Exception ex, string message);
        void LogError(string message);
    }
}
