using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant")]
    public class _AccountNodePartialModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
