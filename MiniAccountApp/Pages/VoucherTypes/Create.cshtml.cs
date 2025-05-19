using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountApp.Data;
using MiniAccountApp.Models;

namespace MiniAccountApp.Pages.VoucherTypes
{
    [Authorize(Roles = "Admin,Accountant")]
    public class CreateModel : PageModel
    {
        private readonly VoucherTypeService _voucherTypeService;

        public CreateModel(VoucherTypeService voucherTypeService)
        {
            _voucherTypeService = voucherTypeService;
        }

        [BindProperty]
        public VoucherType VoucherType { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            VoucherType.CreatedBy = User.Identity?.Name ?? "system";
            _voucherTypeService.CreateVoucherType(VoucherType);

            return RedirectToPage("Index");
        }
    }
}
