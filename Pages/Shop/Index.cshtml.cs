using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using Pharmacy_order_system.Services;

namespace Pharmacy_order_system.Pages.Shop;

[Authorize(Policy = "CustomerOnly")]
public class IndexModel(PharmacyDbContext context, CartService cartService) : PageModel
{
    public List<Medicine> Medicines { get; set; } = [];
    public List<Category> Categories { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await context.Categories.OrderBy(x => x.Name).ToListAsync();

        var query = context.Medicines.Include(x => x.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(x => x.Name.Contains(Search));
        }

        if (CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == CategoryId.Value);
        }

        Medicines = await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int medicineId, int quantity)
    {
        var medicine = await context.Medicines.FindAsync(medicineId);
        if (medicine is null || quantity <= 0)
        {
            return RedirectToPage();
        }

        if (medicine.Stock < quantity)
        {
            TempData["Message"] = "Requested quantity is not available.";
            return RedirectToPage();
        }

        cartService.AddItem(medicine, quantity);

        if (medicine.RequiresPrescription)
        {
            TempData["Message"] = "Medicine added to cart. Please upload your prescription for pharmacist validation.";
            return RedirectToPage("/Prescriptions/Upload", new { fromCart = true });
        }

        return RedirectToPage("/Cart/Index");
    }
}
