using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniAccountApp.Data;
using MiniAccountApp.Models; // Your model namespace
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using System.Data;
using MiniAccountApp.Services;
using System.Configuration;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    public class IndexModel : PageModel
    {
        private readonly ChartOfAccountService _service;
        private readonly IConfiguration _configuration;

        public IndexModel(ChartOfAccountService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        // Key: AccountType, Value: List of top-level accounts with children built
        public Dictionary<string, List<ChartOfAccount>> AccountsByType { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allAccounts = await _service.GetAllAccountsAsync();

            // Build dictionary by AccountType
            var grouped = new Dictionary<string, List<ChartOfAccount>>();

            // First, create a lookup dictionary for all accounts by AccountId
            var lookup = new Dictionary<Guid, ChartOfAccount>();
            foreach (var acc in allAccounts)
            {
                lookup[acc.AccountId] = acc;
                acc.Children = new List<ChartOfAccount>(); // ensure children list
            }

            // Then, assign children to parents
            foreach (var acc in allAccounts)
            {
                if (acc.ParentAccountId.HasValue && lookup.ContainsKey(acc.ParentAccountId.Value))
                {
                    lookup[acc.ParentAccountId.Value].Children.Add(acc);
                }
            }

            // Now, gather top-level accounts (no parent)
            var topLevelAccounts = new List<ChartOfAccount>();
            foreach (var acc in allAccounts)
            {
                if (!acc.ParentAccountId.HasValue)
                {
                    topLevelAccounts.Add(acc);
                }
            }

            // Group top-level accounts by AccountType
            foreach (var acc in topLevelAccounts)
            {
                if (!grouped.ContainsKey(acc.AccountType))
                {
                    grouped[acc.AccountType] = new List<ChartOfAccount>();
                }
                grouped[acc.AccountType].Add(acc);
            }

            // Sort groups by the standard order
            var order = new List<string> { "Assets", "Liabilities", "Equity", "Income", "Expense" };
            var orderedGroups = new Dictionary<string, List<ChartOfAccount>>();
            foreach (var key in order)
            {
                if (grouped.ContainsKey(key))
                {
                    orderedGroups[key] = grouped[key];
                }
            }

            AccountsByType = orderedGroups;
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            // Check if account has children: SELECT COUNT(*) FROM ChartOfAccounts WHERE ParentAccountId = @id
            using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM ChartOfAccounts WHERE ParentAccountId = @ParentId", conn);
            checkCmd.Parameters.AddWithValue("@ParentId", id);
            int childCount = (int)await checkCmd.ExecuteScalarAsync();

            if (childCount > 0)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete account because it has child accounts.");
                await OnGetAsync(); // reload data to redisplay
                return Page();
            }

            // Delete account using stored procedure
            using var deleteCmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            deleteCmd.CommandType = CommandType.StoredProcedure;
            deleteCmd.Parameters.AddWithValue("@Action", "DELETE");
            deleteCmd.Parameters.AddWithValue("@AccountId", id);

            await deleteCmd.ExecuteNonQueryAsync();

            return RedirectToPage();
        }

    }

}
