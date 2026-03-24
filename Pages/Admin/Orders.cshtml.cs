using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;

namespace Pharmacy_order_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class OrdersModel(PharmacyDbContext context) : PageModel
{
    public List<Order> Orders { get; set; } = [];

    public async Task OnGetAsync()
    {
        Orders = await context.Orders
            .Include(x => x.User)
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Medicine)
            .OrderByDescending(x => x.Date)
            .ToListAsync();
    }
}
