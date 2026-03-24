using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using Pharmacy_order_system.Services;
using System.Security.Claims;

namespace Pharmacy_order_system.Pages.Cart;

[Authorize(Policy = "CustomerOnly")]
public class IndexModel(PharmacyDbContext context, CartService cartService) : PageModel
{
    public List<CartItem> Items { get; set; } = [];
    public decimal Total => Items.Sum(x => x.LineTotal);

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? CartMessage { get; set; }

    public void OnGet()
    {
        Items = cartService.GetCart();
    }

    public IActionResult OnPostRemove(int medicineId)
    {
        cartService.RemoveItem(medicineId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        Items = cartService.GetCart();
        if (Items.Count == 0)
        {
            return RedirectToPage();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var needsPrescription = Items.Any(x => x.RequiresPrescription);

        if (needsPrescription)
        {
            var hasApprovedPrescription = await context.Prescriptions.AnyAsync(x => x.UserId == userId && x.Status == PrescriptionStatus.Approved);
            if (!hasApprovedPrescription)
            {
                var hasPendingPrescription = await context.Prescriptions.AnyAsync(x => x.UserId == userId && x.Status == PrescriptionStatus.Pending);
                if (hasPendingPrescription)
                {
                    ErrorMessage = "Your prescription is pending pharmacist approval. Checkout will be available once approved.";
                    return RedirectToPage();
                }

                return RedirectToPage("/Prescriptions/Upload", new { fromCart = true });
            }
        }

        var medicineIds = Items.Select(x => x.MedicineId).ToList();
        var medicines = await context.Medicines.Where(x => medicineIds.Contains(x.MedicineId)).ToDictionaryAsync(x => x.MedicineId);

        foreach (var item in Items)
        {
            if (!medicines.TryGetValue(item.MedicineId, out var medicine) || medicine.Stock < item.Quantity)
            {
                ErrorMessage = $"Insufficient stock for {item.Name}.";
                return RedirectToPage();
            }
        }

        var order = new Order
        {
            UserId = userId,
            Date = DateTime.UtcNow,
            TotalAmount = Total,
            OrderItems = Items.Select(item => new OrderItem
            {
                MedicineId = item.MedicineId,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        };

        foreach (var item in Items)
        {
            medicines[item.MedicineId].Stock -= item.Quantity;
        }

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        cartService.Clear();
        TempData["OrderMessage"] = "Order placed successfully and inventory updated.";
        return RedirectToPage("/Orders/History");
    }
}
