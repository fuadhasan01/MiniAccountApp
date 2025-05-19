using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniAccountApp.Services;
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
            var types = _voucherTypeService.GetAllVoucherTypes(); 
            VoucherTypes = new List<SelectListItem>();
            foreach (var t in types)
            {
                VoucherTypes.Add(new SelectListItem
                {
                    Value = t.VoucherTypeId.ToString(),
                    Text = t.TypeName
                });
            }

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

            Vouchers = _voucherService.GetAllVouchers();
        }
    }
}
