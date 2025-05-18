using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace MiniAccountApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateUserModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateUserModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<string> Roles { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            public string SelectedRole { get; set; }
        }

        public void OnGet()
        {
            // Load roles for dropdown
            Roles = new List<string> { "Admin", "Accountant", "Viewer" };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Roles = new List<string> { "Admin", "Accountant", "Viewer" };

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userExists = await _userManager.FindByEmailAsync(Input.Email);
            if (userExists != null)
            {
                ModelState.AddModelError("", "User with this email already exists.");
                return Page();
            }

            var user = new IdentityUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true // optionally set true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Assign role
                if (await _roleManager.RoleExistsAsync(Input.SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, Input.SelectedRole);
                }
                TempData["SuccessMessage"] = $"User '{Input.Email}' created successfully!";
                return RedirectToPage("Users");
            }
            else
            {
                TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return Page();
        }
    }
}
