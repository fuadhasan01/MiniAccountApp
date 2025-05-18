using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniAccountApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditUserRolesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditUserRolesModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public string UserId { get; set; }

        [BindProperty]
        public List<RoleSelection> Roles { get; set; }

        public class RoleSelection
        {
            public string RoleName { get; set; }
            public bool IsSelected { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            UserId = id;
            var userRoles = await _userManager.GetRolesAsync(user);

            Roles = new List<RoleSelection>();

            foreach (var role in _roleManager.Roles)
            {
                Roles.Add(new RoleSelection
                {
                    RoleName = role.Name,
                    IsSelected = userRoles.Contains(role.Name)
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(UserId))
                return NotFound();

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            // Remove all roles
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            // Add selected roles
            var selectedRoles = new List<string>();
            foreach (var role in Roles)
            {
                if (role.IsSelected)
                    selectedRoles.Add(role.RoleName);
            }
            await _userManager.AddToRolesAsync(user, selectedRoles);

            return RedirectToPage("Users");
        }
    }
}
