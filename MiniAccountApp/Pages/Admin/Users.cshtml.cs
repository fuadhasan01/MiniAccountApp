using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniAccountApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public List<UserWithRoles> Users { get; set; }

        public class UserWithRoles
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public IList<string> Roles { get; set; }
        }

        public async Task OnGetAsync()
        {
            Users = new List<UserWithRoles>();

            var users = _userManager.Users; // IQueryable<IdentityUser>
            // Instead of LINQ, iterate using ToList or foreach

            // ToListAsync requires EF, but since UserManager.Users is IQueryable from EF, you can:
            var userList = new List<IdentityUser>();
            await foreach (var user in users.AsAsyncEnumerable())
            {
                userList.Add(user);
            }

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserWithRoles
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = roles
                });
            }
        }
    }
}
