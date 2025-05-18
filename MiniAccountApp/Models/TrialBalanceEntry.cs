namespace MiniAccountApp.Models
{
    public class TrialBalanceEntry
    {
        public string AccountCode { get; set; } = "";
        public string AccountName { get; set; } = "";
        public string AccountType { get; set; } = "";
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}
