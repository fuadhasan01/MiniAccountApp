using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountApp.Models;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant")]
    public class EditModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public EditModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public ChartOfAccount Account { get; set; }

        public List<ChartOfAccount> ParentAccounts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            ChartOfAccount account;
            using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                cmd.Parameters.AddWithValue("@AccountId", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    account = new ChartOfAccount
                    {
                        AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                        AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                        AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                        AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                        ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? null : reader.GetGuid(reader.GetOrdinal("ParentAccountId")),
                        AccountLevel = reader.GetInt32(reader.GetOrdinal("AccountLevel")),
                        IsActive = reader.GetString(reader.GetOrdinal("IsActive")),
                        AccountNature = reader.IsDBNull(reader.GetOrdinal("AccountNature")) ? null : reader.GetString(reader.GetOrdinal("AccountNature")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    };
                }
                else
                {
                    return NotFound();
                }
                var parents = new List<ChartOfAccount>();
                using (var parentCmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    parentCmd.CommandType = CommandType.StoredProcedure;
                    parentCmd.Parameters.AddWithValue("@Action", "SELECT");

                    using var parentReader = await parentCmd.ExecuteReaderAsync();
                    while (await parentReader.ReadAsync())
                    {
                        var parentAccountId = parentReader.GetGuid(parentReader.GetOrdinal("AccountId"));
                        if (parentAccountId != id)
                        {
                            parents.Add(new ChartOfAccount
                            {
                                AccountId = parentAccountId,
                                AccountCode = parentReader.GetString(parentReader.GetOrdinal("AccountCode")),
                                AccountName = parentReader.GetString(parentReader.GetOrdinal("AccountName"))
                            });
                        }
                    }
                }

                Account = account;
                ParentAccounts = parents;

                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "UPDATE");
            cmd.Parameters.AddWithValue("@AccountId", Account.AccountId);
            cmd.Parameters.AddWithValue("@AccountCode", Account.AccountCode ?? "");
            cmd.Parameters.AddWithValue("@AccountName", Account.AccountName ?? "");
            cmd.Parameters.AddWithValue("@AccountType", Account.AccountType ?? "");
            cmd.Parameters.AddWithValue("@ParentAccountId", (object?)Account.ParentAccountId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountLevel", Account.AccountLevel);
            cmd.Parameters.AddWithValue("@IsActive", Account.IsActive ?? "");
            cmd.Parameters.AddWithValue("@AccountNature", Account.AccountNature ?? "");
            cmd.Parameters.AddWithValue("@UpdatedBy", "system");
            cmd.Parameters.AddWithValue("@Description", Account.Description ?? "");

            await cmd.ExecuteNonQueryAsync();

            return RedirectToPage("./Index");
        }
    }

}
