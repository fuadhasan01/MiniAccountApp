using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountApp.Data;
using MiniAccountApp.Models;
using MiniAccountApp.Services;
using System;
using System.Collections.Generic;

namespace MiniAccountApp.Pages.Reports
{
    public class VoucherSummaryModel : PageModel
    {
        private readonly VoucherService _voucherService;

        public VoucherSummaryModel(VoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        public List<VoucherSummaryDto> Summaries { get; set; } = new();

        [BindProperty]
        public DateTime? StartDate { get; set; }

        [BindProperty]
        public DateTime? EndDate { get; set; }

        public void OnGet()
        {
            Summaries = _voucherService.GetVoucherSummary(StartDate, EndDate);
        }

        public void OnPost()
        {
            Summaries = _voucherService.GetVoucherSummary(StartDate, EndDate);
        }
    }
}
