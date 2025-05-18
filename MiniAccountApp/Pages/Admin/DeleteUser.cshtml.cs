using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace MiniAccountApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DeleteUserModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public DeleteUserModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string UserId { get; set; }

        public string Email { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            UserId = id;
            Email = user.Email;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(UserId))
                return NotFound();

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error deleting user.");
                return Page();
            }

            return RedirectToPage("Users");
        }
    }
}
