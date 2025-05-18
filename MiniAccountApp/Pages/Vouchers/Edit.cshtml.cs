using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniAccountApp.Models;
using MiniAccountApp.Data;
using System.Data.SqlClient;
using System.Data;
using MiniAccountApp.Services;
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountApp.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
    public class EditModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ChartOfAccountService _coaService;
        private readonly VoucherTypeService _voucherTypeService;
        private readonly VoucherService _voucherService;

        public EditModel(IConfiguration config, ChartOfAccountService coaService, VoucherTypeService voucherTypeService, VoucherService voucherService)
        {
            _config = config;
            _coaService = coaService;
            _voucherTypeService = voucherTypeService;
            _voucherService = voucherService;
        }

        [BindProperty]
        public VoucherCreateDto Voucher { get; set; }

        public List<SelectListItem> VoucherTypes { get; set; }
        public List<SelectListItem> AccountList { get; set; }

        public IActionResult OnGet(Guid id)
        {
            // 1. Dropdown Bind
            VoucherTypes = _voucherTypeService.GetAllVoucherTypes()
                .Select(v => new SelectListItem { Value = v.VoucherTypeId.ToString(), Text = v.TypeName }).ToList();

            AccountList = _coaService.GetAllAccountsAsync().GetAwaiter().GetResult()
                .Select(a => new SelectListItem
                {
                    Value = a.AccountId.ToString(),
                    Text = $"{a.AccountCode} - {a.AccountName}"
                }).ToList();

            // 2. Get Voucher by ID
            Voucher = _voucherService.GetVoucherById(id);
            if (Voucher == null)
            {
                return RedirectToPage("Index");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            _voucherService.UpdateVoucher(Voucher);
            TempData["SuccessMessage"] = $"Voucher {Voucher.VoucherNo} updated successfully!";
            return RedirectToPage("Index");
        }
    }
}
