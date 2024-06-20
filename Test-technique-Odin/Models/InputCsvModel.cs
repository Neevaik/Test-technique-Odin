namespace Test_technique_Odin.Models
{
    public class InputCsvModel
    {
        public string OrderId { get; set; }
        public string Nature { get; set; }
        public string OperationType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal BillingFees { get; set; }
        public DateTime Date { get; set; }
        public string ChargebackDate { get; set; }
        public string TransferReference { get; set; }
        public string ExtraData { get; set; }
        public decimal SecureFee { get; set; }
    }
}
