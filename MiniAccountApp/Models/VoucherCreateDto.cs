namespace MiniAccountApp.Models
{
    public class VoucherCreateDto
    {
        public Guid VoucherId { get; set; }
        public Guid VoucherTypeId { get; set; }
        public DateTime VoucherDate { get; set; }
        public string? VoucherNo { get; set; }
        public string? Remarks { get; set; }
        public List<VoucherEntryDto> Entries { get; set; } = new();
    }

    public class VoucherEntryDto
    {
        public Guid AccountId { get; set; }
        public string? Particulars { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
    }

}
