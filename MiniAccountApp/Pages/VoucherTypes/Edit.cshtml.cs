using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountApp.Data;
using MiniAccountApp.Models;
using System;

namespace MiniAccountApp.Pages.VoucherTypes
{
    public class EditModel : PageModel
    {
        private readonly VoucherTypeService _voucherTypeService;

        public EditModel(VoucherTypeService voucherTypeService)
        {
            _voucherTypeService = voucherTypeService;
        }

        [BindProperty]
        public VoucherType VoucherType { get; set; }

        public IActionResult OnGet(Guid id)
        {
            VoucherType = _voucherTypeService.GetVoucherTypeById(id);

            if (VoucherType == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            VoucherType.UpdatedBy = User.Identity?.Name ?? "system";
            _voucherTypeService.UpdateVoucherType(VoucherType);

            return RedirectToPage("Index");
        }
    }
}
