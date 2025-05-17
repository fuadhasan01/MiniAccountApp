using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MiniAccountApp.Data;
using MiniAccountApp.Models;
using System.Collections.Generic;

namespace MiniAccountApp.Pages.VoucherTypes
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly VoucherTypeService _voucherTypeService;

        public IndexModel(ILogger<IndexModel> logger, VoucherTypeService voucherTypeService)
        {
            _logger = logger;
            _voucherTypeService = voucherTypeService;
        }

        public List<VoucherType> VoucherTypes { get; set; }

        public void OnGet()
        {
            VoucherTypes = _voucherTypeService.GetAllVoucherTypes();
        }
        public IActionResult OnPostDelete(Guid id)
        {
            _voucherTypeService.DeleteVoucherType(id);
            return RedirectToPage();
        }
    }
}
