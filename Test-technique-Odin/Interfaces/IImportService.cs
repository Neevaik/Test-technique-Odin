namespace Test_technique_Odin.Interfaces
{
    public interface IImportService
    {
        DateTime Date { get; set; }
        Decimal TotalAmount { get; set; }
        Decimal TotalBillingFee { get; set; }
        Decimal Total3dsFee { get; set; }
    }
}
