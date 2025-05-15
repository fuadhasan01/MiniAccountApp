using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public ChartOfAccount Account { get; set; } = new ChartOfAccount
        {
            AccountLevel = 1,
            IsActive = "Active"
        };
        public List<SelectListItem> AccountTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem("Asset", "Asset"),
            new SelectListItem("Liability", "Liability"),
            new SelectListItem("Equity", "Equity"),
            new SelectListItem("Revenue", "Revenue"),
            new SelectListItem("Expense", "Expense")
        };
        public List<SelectListItem> IsActiveOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem("Active", "Active"),
            new SelectListItem("Inactive", "Inactive")
        };
        public List<SelectListItem> AccountNatureOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem("Debit", "Debit"),
            new SelectListItem("Credit", "Credit")
        };
        public void OnGet()
        {
            // No data required for GET
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Account.CreatedBy = User.Identity.Name ?? "System";
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "usp_CreateChartOfAccount";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@AccountCode", Account.AccountCode));
                command.Parameters.Add(new SqlParameter("@AccountName", Account.AccountName));
                command.Parameters.Add(new SqlParameter("@AccountType", Account.AccountType));
                command.Parameters.Add(new SqlParameter("@AccountCategory", (object?)Account.AccountCategory ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@ParentAccountId", (object?)Account.ParentAccountId ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@AccountLevel", Account.AccountLevel));
                command.Parameters.Add(new SqlParameter("@IsActive", Account.IsActive));
                command.Parameters.Add(new SqlParameter("@AccountNature", (object?)Account.AccountNature ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@CreatedBy", (object?)Account.CreatedBy ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@Description", (object?)Account.Description ?? DBNull.Value));

                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();

                TempData["SuccessMessage"] = "Account created successfully!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // Log error (not shown here)
                ModelState.AddModelError(string.Empty, $"Error saving data: {ex.Message}");
                return Page();
            }
        }
    }
}
