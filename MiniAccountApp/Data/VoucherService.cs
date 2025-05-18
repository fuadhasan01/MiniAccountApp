using Microsoft.Data.SqlClient;
using MiniAccountApp.Models;
using MiniAccountApp.Pages.Vouchers;
using System.Data;

namespace MiniAccountApp.Data
{
    public class VoucherService
    {
        private readonly IConfiguration _configuration;

        public VoucherService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateVoucher(VoucherCreateDto dto)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageVouchers", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            // Required Parameters
            cmd.Parameters.AddWithValue("@Action", "CREATE");
            cmd.Parameters.AddWithValue("@VoucherTypeId", dto.VoucherTypeId);
            cmd.Parameters.AddWithValue("@VoucherDate", dto.VoucherDate);
            cmd.Parameters.AddWithValue("@Remarks", dto.Remarks ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", "system");

            // OUTPUT Parameter for VoucherNo
            var voucherNoParam = new SqlParameter("@VoucherNo", SqlDbType.NVarChar, 50)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(voucherNoParam);

            // TVP: Voucher Entries
            var entryTable = new DataTable();
            entryTable.Columns.Add("AccountId", typeof(Guid));
            entryTable.Columns.Add("Particulars", typeof(string));
            entryTable.Columns.Add("DebitAmount", typeof(decimal));
            entryTable.Columns.Add("CreditAmount", typeof(decimal));

            foreach (var entry in dto.Entries)
            {
                entryTable.Rows.Add(
                    entry.AccountId,
                    entry.Particulars ?? (object)DBNull.Value,
                    entry.DebitAmount.HasValue ? entry.DebitAmount.Value : (object)DBNull.Value,
                    entry.CreditAmount.HasValue ? entry.CreditAmount.Value : (object)DBNull.Value
                );
            }

            var tvpParam = cmd.Parameters.AddWithValue("@Entries", entryTable);
            tvpParam.SqlDbType = SqlDbType.Structured;
            tvpParam.TypeName = "dbo.VoucherEntryTableType";

            // Execute
            conn.Open();
            cmd.ExecuteNonQuery();

            // Get the generated Voucher No
            string newVoucherNo = voucherNoParam.Value?.ToString();
            return newVoucherNo ?? string.Empty;
        }

        public VoucherCreateDto GetVoucherById(Guid id)
        {
            var dto = new VoucherCreateDto();
            dto.Entries = new List<VoucherEntryDto>();
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageVouchers", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GETBYID");
            cmd.Parameters.AddWithValue("@VoucherId", id);
            cmd.Parameters.AddWithValue("@VoucherNo", DBNull.Value);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                dto.VoucherId = id;
                dto.VoucherTypeId = reader.GetGuid(reader.GetOrdinal("VoucherTypeId"));
                dto.VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate"));
                dto.Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks"));
                dto.VoucherNo = reader.GetString(reader.GetOrdinal("VoucherNo"));
                dto.VoucherTypeName = reader.IsDBNull(reader.GetOrdinal("TypeName")) ? null : reader.GetString(reader.GetOrdinal("TypeName"));

            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    dto.Entries.Add(new VoucherEntryDto
                    {
                        AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                        Particulars = reader["Particulars"]?.ToString(),
                        DebitAmount = reader["DebitAmount"] as decimal?,
                        CreditAmount = reader["CreditAmount"] as decimal?
                    });
                }
            }

            return dto;
        }

        public void UpdateVoucher(VoucherCreateDto dto)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageVouchers", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "UPDATE");
            cmd.Parameters.AddWithValue("@VoucherId", dto.VoucherId);
            cmd.Parameters.AddWithValue("@VoucherTypeId", dto.VoucherTypeId);
            cmd.Parameters.AddWithValue("@VoucherDate", dto.VoucherDate);
            cmd.Parameters.AddWithValue("@VoucherNo", dto.VoucherNo);
            cmd.Parameters.AddWithValue("@Remarks", dto.Remarks ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedBy", "system");

            // Entries
            var table = new DataTable();
            table.Columns.Add("AccountId", typeof(Guid));
            table.Columns.Add("Particulars", typeof(string));
            table.Columns.Add("DebitAmount", typeof(decimal));
            table.Columns.Add("CreditAmount", typeof(decimal));
            foreach (var e in dto.Entries)
            {
                object debit = DBNull.Value;
                object credit = DBNull.Value;

                if (e.DebitAmount.HasValue && e.DebitAmount.Value > 0)
                    debit = e.DebitAmount.Value;

                if (e.CreditAmount.HasValue && e.CreditAmount.Value > 0)
                    credit = e.CreditAmount.Value;

                if (debit != DBNull.Value && credit != DBNull.Value)
                    throw new Exception("A voucher entry cannot have both Debit and Credit.");

                if (debit == DBNull.Value && credit == DBNull.Value)
                    throw new Exception("A voucher entry must have either Debit or Credit.");

                table.Rows.Add(
                    e.AccountId,
                    e.Particulars ?? (object)DBNull.Value,
                    debit,
                    credit
                );
            }


            var tvpParam = cmd.Parameters.AddWithValue("@Entries", table);
            tvpParam.SqlDbType = SqlDbType.Structured;
            tvpParam.TypeName = "dbo.VoucherEntryTableType";

            conn.Open();
            cmd.ExecuteNonQuery();
        }
        public List<VoucherListDto> GetAllVouchers()
        {
            var list = new List<VoucherListDto>();
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageVouchers", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "GETALL");

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new VoucherListDto
                {
                    VoucherId = reader.GetGuid(reader.GetOrdinal("VoucherId")),
                    VoucherNo = reader.GetString(reader.GetOrdinal("VoucherNo")),
                    VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
                    Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? "" : reader.GetString(reader.GetOrdinal("Remarks")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    VoucherTypeId = reader.GetGuid(reader.GetOrdinal("VoucherTypeId")),
                    VoucherTypeName = reader.IsDBNull(reader.GetOrdinal("TypeName"))
                ? "" : reader.GetString(reader.GetOrdinal("TypeName"))
                });
            }
            return list;
        }


    }
}
