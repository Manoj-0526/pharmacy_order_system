using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;

namespace Pharmacy_order_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class UsersModel(PharmacyDbContext context) : PageModel
{
    public List<User> Users { get; set; } = [];

    public async Task OnGetAsync()
    {
        Users = await context.Users.OrderBy(x => x.Role).ThenBy(x => x.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var user = await context.Users.FirstOrDefaultAsync(x =>
            x.UserId == id &&
            (x.Role == UserRole.Staff || x.Role == UserRole.Pharmacist));

        if (user is not null)
        {
            user.IsApproved = true;
            await context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
