using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using Pharmacy_order_system.Services;

namespace Pharmacy_order_system.Pages.Shop;

[Authorize(Policy = "CustomerOnly")]
public class DetailsModel(PharmacyDbContext context, CartService cartService) : PageModel
{
    public Medicine? Medicine { get; set; }

    public async Task OnGetAsync(int id)
    {
        Medicine = await context.Medicines.Include(x => x.Category).FirstOrDefaultAsync(x => x.MedicineId == id);
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int id, int quantity)
    {
        var medicine = await context.Medicines.FindAsync(id);
        if (medicine is null || quantity <= 0 || medicine.Stock < quantity)
        {
            return RedirectToPage(new { id });
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
