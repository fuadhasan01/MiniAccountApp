using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    public class ListModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ListModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ChartOfAccount> Accounts { get; set; } = new();
        public List<string> AccountTypes { get; set; } = new() { "Assets", "Liabilities", "Equity", "Income", "Expense" };

        // Filters bound from query string
        public string FilterAccountType { get; set; }
        public string FilterAccountName { get; set; }

        public async Task OnGetAsync(string accountType, string accountName)
        {
            FilterAccountType = accountType;
            FilterAccountName = accountName;

            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "SELECT");

            using var reader = await cmd.ExecuteReaderAsync();

            var tempAccounts = new List<ChartOfAccount>();
            while (await reader.ReadAsync())
            {
                tempAccounts.Add(new ChartOfAccount
                {
                    AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                    AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                    AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                    ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? null : reader.GetGuid(reader.GetOrdinal("ParentAccountId")),
                    IsActive = reader.GetString(reader.GetOrdinal("IsActive")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                });
            }

            var accountDict = new Dictionary<Guid, ChartOfAccount>();
            foreach (var acc in tempAccounts)
                accountDict[acc.AccountId] = acc;

            foreach (var acc in tempAccounts)
                acc.ParentAccountName = acc.ParentAccountId.HasValue && accountDict.ContainsKey(acc.ParentAccountId.Value)
                    ? accountDict[acc.ParentAccountId.Value].AccountName : "";

            // Manual filtering without LINQ
            var filteredAccounts = new List<ChartOfAccount>();
            foreach (var acc in tempAccounts)
            {
                bool matchesType = string.IsNullOrEmpty(FilterAccountType) || acc.AccountType == FilterAccountType;
                bool matchesName = string.IsNullOrEmpty(FilterAccountName) ||
                                   acc.AccountName.IndexOf(FilterAccountName, StringComparison.OrdinalIgnoreCase) >= 0;

                if (matchesType && matchesName)
                    filteredAccounts.Add(acc);
            }

            // Order accounts by fixed AccountType order manually
            var order = new List<string> { "Assets", "Liabilities", "Equity", "Income", "Expense" };
            var orderedAccounts = new List<ChartOfAccount>();
            foreach (var type in order)
            {
                foreach (var acc in filteredAccounts)
                {
                    if (acc.AccountType == type)
                        orderedAccounts.Add(acc);
                }
            }

            Accounts = orderedAccounts;
        }


    }
}
