using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;

namespace Pharmacy_order_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class DashboardModel(PharmacyDbContext context) : PageModel
{
    public int TotalUsers { get; set; }
    public int PendingStaff { get; set; }
    public int TotalMedicines { get; set; }
    public int TotalOrders { get; set; }

    public async Task OnGetAsync()
    {
        TotalUsers = await context.Users.CountAsync();
        PendingStaff = await context.Users.CountAsync(x => x.Role == UserRole.Staff && !x.IsApproved);
        TotalMedicines = await context.Medicines.CountAsync();
        TotalOrders = await context.Orders.CountAsync();
    }
}
