using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountApp.Models;
using MiniAccountApp.Services;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    public class CreateModel : PageModel
    {
        private readonly ChartOfAccountService _service;

        public CreateModel(ChartOfAccountService service)
        {
            _service = service;
        }

        [BindProperty]
        public ChartOfAccount ChartOfAccount { get; set; }

        public SelectList ParentAccounts { get; set; }

        public async Task OnGetAsync()
        {
            var accounts = await _service.GetAllAccountsAsync();
            ParentAccounts = new SelectList(accounts, "AccountId", "AccountName");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);

                await OnGetAsync();  // reload parent accounts for dropdown if error
                return Page();
            }

            // Set CreatedBy to current user or system default
            ChartOfAccount.CreatedBy = User.Identity?.Name ?? "system";

            // Calculate AccountLevel based on parent
            if (ChartOfAccount.ParentAccountId.HasValue)
            {
                var parent = await _service.GetAccountByIdAsync(ChartOfAccount.ParentAccountId.Value);
                ChartOfAccount.AccountLevel = parent.AccountLevel + 1;
            }
            else
            {
                ChartOfAccount.AccountLevel = 1;
            }

            await _service.CreateAsync(ChartOfAccount);

            return RedirectToPage("Index");
        }
    }
}
