using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniAccountApp.Services;  // your services namespace
using System.Collections.Generic;
using System;
using MiniAccountApp.Data;
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountApp.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
    public class ListModel : PageModel
    {
        private readonly VoucherTypeService _voucherTypeService;
        private readonly ChartOfAccountService _coaService;
        private readonly VoucherService _voucherService;

        public ListModel(VoucherTypeService voucherTypeService, ChartOfAccountService coaService, VoucherService voucherService)
        {
            _voucherTypeService = voucherTypeService;
            _coaService = coaService;
            _voucherService = voucherService;
        }

        public List<SelectListItem> VoucherTypes { get; set; } = new();
        public List<SelectListItem> AccountList { get; set; } = new();

        public List<VoucherListDto> Vouchers { get; set; } = new();

        public void OnGet()
        {
            // Load Voucher Types
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

            // Load Chart of Accounts
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

            // Load vouchers - you can call your VoucherService or sp_ManageVouchers GETALL here
            // (Assuming VoucherService has a method GetAllVouchers returning List<VoucherListDto>)
            Vouchers = _voucherService.GetAllVouchers();
        }
    }
}
