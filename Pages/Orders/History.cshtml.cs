using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using System.Security.Claims;

namespace Pharmacy_order_system.Pages.Orders;

[Authorize(Policy = "CustomerOnly")]
public class HistoryModel(PharmacyDbContext context) : PageModel
{
    public List<Order> Orders { get; set; } = [];

    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        Orders = await context.Orders
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Medicine)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ToListAsync();
    }
}
