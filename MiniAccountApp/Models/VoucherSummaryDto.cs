namespace MiniAccountApp.Models
{
    public class VoucherSummaryDto
    {
        public Guid VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherTypeName { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string Remarks { get; set; }
    }

}
