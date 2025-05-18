using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniAccountApp.Data;
using MiniAccountApp.Models;
using MiniAccountApp.Services;
using System.Collections.Generic;

namespace MiniAccountApp.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
    public class CreateModel : PageModel
    {
        private readonly VoucherTypeService _voucherTypeService;
        private readonly VoucherService _voucherService;
        private readonly ChartOfAccountService _coaService;


        public CreateModel(VoucherTypeService voucherTypeService, VoucherService voucherService, ChartOfAccountService chartOfAccountService)
        {
            _voucherTypeService = voucherTypeService;
            _voucherService = voucherService;
            _coaService = chartOfAccountService;
        }

        [BindProperty]
        public VoucherCreateDto Voucher { get; set; }

        public List<SelectListItem> VoucherTypes { get; set; }
        public List<SelectListItem> AccountList { get; set; }


        public IActionResult OnGet()
        {
            // Voucher Types
            var types = _voucherTypeService.GetAllVoucherTypes(); // List<VoucherType>
            VoucherTypes = new List<SelectListItem>();

            foreach (var t in types)
            {
                VoucherTypes.Add(new SelectListItem
                {
                    Value = t.VoucherTypeId.ToString(),
                    Text = t.TypeName
                });
            }

            // Chart of Accounts
            var accounts = _coaService.GetAllAccountsAsync().GetAwaiter().GetResult(); // List<ChartOfAccount>
            AccountList = new List<SelectListItem>();

            foreach (var a in accounts)
            {
                AccountList.Add(new SelectListItem
                {
                    Value = a.AccountId.ToString(),
                    Text = a.AccountCode + " - " + a.AccountName
                });
            }

            return Page();
        }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var voucherNo = _voucherService.CreateVoucher(Voucher);
            TempData["SuccessMessage"] = $"Voucher created successfully. Voucher No: {voucherNo}";
            return RedirectToPage("Index");
        }
    }
}
