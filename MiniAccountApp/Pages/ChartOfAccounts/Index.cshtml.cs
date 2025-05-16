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

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    public class IndexModel : PageModel
    {
        private readonly ChartOfAccountService _service;

        public IndexModel(ChartOfAccountService service)
        {
            _service = service;
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
    }

}
