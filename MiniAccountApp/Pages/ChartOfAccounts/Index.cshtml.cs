using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniAccountApp.Data;
using MiniAccountApp.Models; // Your model namespace
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<ChartOfAccount> AccountTree { get; set; } = new();

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            var accounts = await LoadAllAccountsAsync();
            AccountTree = BuildAccountTree(accounts);
        }

        private async Task<List<ChartOfAccount>> LoadAllAccountsAsync()
        {
            var list = new List<ChartOfAccount>();

            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("SELECT * FROM ChartOfAccounts ORDER BY AccountCode", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ChartOfAccount
                {
                    AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                    AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                    AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                    ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? null : reader.GetGuid(reader.GetOrdinal("ParentAccountId")),
                    AccountLevel = reader.GetInt32(reader.GetOrdinal("AccountLevel")),
                    IsActive = reader.GetString(reader.GetOrdinal("IsActive")),
                    AccountNature = reader.IsDBNull(reader.GetOrdinal("AccountNature")) ? null : reader.GetString(reader.GetOrdinal("AccountNature")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                });
            }
            return list;
        }

        // Build tree from flat list
        private List<ChartOfAccount> BuildAccountTree(List<ChartOfAccount> accounts)
        {
            var lookup = accounts.ToDictionary(a => a.AccountId);
            var roots = new List<ChartOfAccount>();

            foreach (var account in accounts)
            {
                if (account.ParentAccountId != null && lookup.TryGetValue(account.ParentAccountId.Value, out var parent))
                {
                    parent.Children.Add(account);
                }
                else
                {
                    roots.Add(account);
                }
            }

            return roots;
        }

        // Example: Manage account method to call stored procedure
        public async Task ManageAccountAsync(string action, ChartOfAccount account, string userName)
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@AccountId", action == "INSERT" ? DBNull.Value : account.AccountId);
            cmd.Parameters.AddWithValue("@AccountCode", account.AccountCode ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountName", account.AccountName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountType", account.AccountType ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ParentAccountId", account.ParentAccountId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountLevel", account.AccountLevel);
            cmd.Parameters.AddWithValue("@IsActive", account.IsActive ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountNature", account.AccountNature ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", action == "INSERT" ? userName : DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedBy", action == "UPDATE" ? userName : DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", account.Description ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

    }
}
