using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountApp.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace MiniAccountApp.Pages.Vouchers
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<VoucherListDto> Vouchers { get; set; }

        public void OnGet()
        {
            Vouchers = new List<VoucherListDto>();
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_ManageVouchers", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GETALL");

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Vouchers.Add(new VoucherListDto
                {
                    VoucherId = reader.GetGuid(reader.GetOrdinal("VoucherId")),
                    VoucherNo = reader.GetString(reader.GetOrdinal("VoucherNo")),
                    VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
                    Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? "" : reader.GetString(reader.GetOrdinal("Remarks")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
                });
            }
        }
    }

    public class VoucherListDto
    {
        public Guid VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
