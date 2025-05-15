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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ChartOfAccount Account { get; set; }

        public void OnGet()
        {
            // No data required for GET
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Call stored procedure to insert the new account
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "usp_CreateChartOfAccount";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@AccountCode", Account.AccountCode));
                command.Parameters.Add(new SqlParameter("@AccountName", Account.AccountName));
                command.Parameters.Add(new SqlParameter("@AccountType", Account.AccountType));
                command.Parameters.Add(new SqlParameter("@ParentAccountId", (object?)Account.ParentAccountId ?? DBNull.Value));

                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }

            return RedirectToPage("./Index"); // Redirect to listing page after create
        }
    }
}
