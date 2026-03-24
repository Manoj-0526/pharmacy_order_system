using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using System.Security.Claims;

namespace Pharmacy_order_system.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(PharmacyDbContext context) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> MyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var orders = await context.Orders
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Medicine)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllOrders()
    {
        var orders = await context.Orders
            .Include(x => x.User)
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Medicine)
            .OrderByDescending(x => x.Date)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("At least one item is required.");
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var medicineIds = request.Items.Select(x => x.MedicineId).ToList();
        var medicines = await context.Medicines.Where(x => medicineIds.Contains(x.MedicineId)).ToDictionaryAsync(x => x.MedicineId);

        foreach (var item in request.Items)
        {
            if (!medicines.TryGetValue(item.MedicineId, out var medicine) || medicine.Stock < item.Quantity)
            {
                return BadRequest($"Insufficient stock for medicineId: {item.MedicineId}");
            }
        }

        var total = request.Items.Sum(x => medicines[x.MedicineId].Price * x.Quantity);

        var order = new Order
        {
            UserId = userId,
            TotalAmount = total,
            Date = DateTime.UtcNow,
            OrderItems = request.Items.Select(x => new OrderItem
            {
                MedicineId = x.MedicineId,
                Quantity = x.Quantity,
                Price = medicines[x.MedicineId].Price
            }).ToList()
        };

        foreach (var item in request.Items)
        {
            medicines[item.MedicineId].Stock -= item.Quantity;
        }

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return Ok(new { order.OrderId, order.TotalAmount, order.Date });
    }

    public class CreateOrderRequest
    {
        public List<OrderLineRequest> Items { get; set; } = [];
    }

    public class OrderLineRequest
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }
}
