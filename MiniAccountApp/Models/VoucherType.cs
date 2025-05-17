namespace MiniAccountApp.Models
{
    public class VoucherType
    {
        public Guid VoucherTypeId { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Description { get; set; }
    }

}
