using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountApp.Services;
using MiniAccountApp.Models;
using System;
using System.Collections.Generic;
using MiniAccountApp.Data;
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountApp.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
    public class ViewModel : PageModel
    {
        private readonly VoucherService _voucherService;
        private readonly ChartOfAccountService _coaService;

        public ViewModel(VoucherService voucherService, ChartOfAccountService coaService)
        {
            _voucherService = voucherService;
            _coaService = coaService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }  

        public VoucherCreateDto Voucher { get; set; }
        public Dictionary<Guid, string> AccountNames { get; set; } = new();

        public IActionResult OnGet()
        {
            if (Id == Guid.Empty)
                return RedirectToPage("/Vouchers/List");

            Voucher = _voucherService.GetVoucherById(Id);
            if (Voucher == null)
                return RedirectToPage("/Vouchers/List");

            var accounts = _coaService.GetAllAccountsAsync().GetAwaiter().GetResult();
            foreach (var a in accounts)
            {
                AccountNames[a.AccountId] = $"{a.AccountCode} - {a.AccountName}";
            }

            return Page();
        }
    }
}
