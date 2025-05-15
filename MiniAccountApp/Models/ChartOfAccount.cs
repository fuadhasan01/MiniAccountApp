namespace MiniAccountApp.Models
{
    public class ChartOfAccount
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public int? ParentAccountId { get; set; }
    }
}
