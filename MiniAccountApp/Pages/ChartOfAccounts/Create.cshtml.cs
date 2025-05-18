using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountApp.Models;
using MiniAccountApp.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant")]
    public class CreateModel : PageModel
    {
        private readonly ChartOfAccountService _service;
        private readonly IConfiguration _configuration;
        public CreateModel(ChartOfAccountService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        [BindProperty]
        public ChartOfAccount ChartOfAccount { get; set; }

        public SelectList ParentAccounts { get; set; }

        public async Task OnGetAsync()
        {
            var parents = new List<ChartOfAccount>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                using (var parentCmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    parentCmd.CommandType = CommandType.StoredProcedure;
                    parentCmd.Parameters.AddWithValue("@Action", "SELECT");

                    using var parentReader = await parentCmd.ExecuteReaderAsync();

                    while (await parentReader.ReadAsync())
                    {
                        var parentAccountId = parentReader.GetGuid(parentReader.GetOrdinal("AccountId"));

                        parents.Add(new ChartOfAccount
                        {
                            AccountId = parentAccountId,
                            AccountCode = parentReader.GetString(parentReader.GetOrdinal("AccountCode")),
                            AccountName = parentReader.GetString(parentReader.GetOrdinal("AccountName"))
                        });
                    }
                }
            }

            var parentSelectListItems = new List<SelectListItem>();

            foreach (var p in parents)
            {
                parentSelectListItems.Add(new SelectListItem
                {
                    Value = p.AccountId.ToString(),
                    Text = p.AccountCode + " - " + p.AccountName
                });
            }

            ParentAccounts = new SelectList(parentSelectListItems, "Value", "Text");
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
