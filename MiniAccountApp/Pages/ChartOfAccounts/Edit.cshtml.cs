using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniAccountApp.Data;
using MiniAccountApp.Models;
using System.Data;
using System.Data.SqlClient;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ChartOfAccount Account { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "usp_GetChartOfAccountById";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@AccountId", id));

            await _context.Database.OpenConnectionAsync();

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Account = new ChartOfAccount
                {
                    AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                    AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                    AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                    ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId"))
                        ? (int?)null
                        : reader.GetInt32(reader.GetOrdinal("ParentAccountId"))
                };
            }
            else
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "usp_UpdateChartOfAccount";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@AccountId", Account.AccountId));
            command.Parameters.Add(new SqlParameter("@AccountCode", Account.AccountCode));
            command.Parameters.Add(new SqlParameter("@AccountName", Account.AccountName));
            command.Parameters.Add(new SqlParameter("@AccountType", Account.AccountType));
            command.Parameters.Add(new SqlParameter("@ParentAccountId", (object?)Account.ParentAccountId ?? DBNull.Value));

            await _context.Database.OpenConnectionAsync();
            await command.ExecuteNonQueryAsync();

            return RedirectToPage("Index");
        }
    }
}
