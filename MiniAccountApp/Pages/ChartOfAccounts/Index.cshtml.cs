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
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant")]
    public class IndexModel : PageModel
    {
        private readonly ChartOfAccountService _service;
        private readonly IConfiguration _configuration;

        public IndexModel(ChartOfAccountService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        public Dictionary<string, List<ChartOfAccount>> AccountsByType { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allAccounts = await _service.GetAllAccountsAsync();

            var grouped = new Dictionary<string, List<ChartOfAccount>>();

            var lookup = new Dictionary<Guid, ChartOfAccount>();
            foreach (var acc in allAccounts)
            {
                lookup[acc.AccountId] = acc;
                acc.Children = new List<ChartOfAccount>(); 
            }

            foreach (var acc in allAccounts)
            {
                if (acc.ParentAccountId.HasValue && lookup.ContainsKey(acc.ParentAccountId.Value))
                {
                    lookup[acc.ParentAccountId.Value].Children.Add(acc);
                }
            }

            var topLevelAccounts = new List<ChartOfAccount>();
            foreach (var acc in allAccounts)
            {
                if (!acc.ParentAccountId.HasValue)
                {
                    topLevelAccounts.Add(acc);
                }
            }

            foreach (var acc in topLevelAccounts)
            {
                if (!grouped.ContainsKey(acc.AccountType))
                {
                    grouped[acc.AccountType] = new List<ChartOfAccount>();
                }
                grouped[acc.AccountType].Add(acc);
            }

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

            using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM ChartOfAccounts WHERE ParentAccountId = @ParentId", conn);
            checkCmd.Parameters.AddWithValue("@ParentId", id);
            int childCount = (int)await checkCmd.ExecuteScalarAsync();

            if (childCount > 0)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete account because it has child accounts.");
                await OnGetAsync(); 
                return Page();
            }

            using var deleteCmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            deleteCmd.CommandType = CommandType.StoredProcedure;
            deleteCmd.Parameters.AddWithValue("@Action", "DELETE");
            deleteCmd.Parameters.AddWithValue("@AccountId", id);

            await deleteCmd.ExecuteNonQueryAsync();

            return RedirectToPage();
        }

    }

}
