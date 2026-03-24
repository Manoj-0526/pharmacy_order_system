using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_order_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class EditMedicineModel(PharmacyDbContext context) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> CategoryItems { get; set; } = [];

    public class InputModel
    {
        public int MedicineId { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        [Required] public string Dosage { get; set; } = string.Empty;
        [Required] public string Packaging { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool RequiresPrescription { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var medicine = await context.Medicines.FindAsync(id);
        if (medicine is null)
        {
            return RedirectToPage("/Admin/Medicines");
        }

        Input = new InputModel
        {
            MedicineId = medicine.MedicineId,
            Name = medicine.Name,
            CategoryId = medicine.CategoryId,
            Dosage = medicine.Dosage,
            Packaging = medicine.Packaging,
            Price = medicine.Price,
            Stock = medicine.Stock,
            RequiresPrescription = medicine.RequiresPrescription
        };

        await LoadCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        var medicine = await context.Medicines.FindAsync(Input.MedicineId);
        if (medicine is null)
        {
            return RedirectToPage("/Admin/Medicines");
        }

        medicine.Name = Input.Name;
        medicine.CategoryId = Input.CategoryId;
        medicine.Dosage = Input.Dosage;
        medicine.Packaging = Input.Packaging;
        medicine.Price = Input.Price;
        medicine.Stock = Input.Stock;
        medicine.RequiresPrescription = Input.RequiresPrescription;

        await context.SaveChangesAsync();
        return RedirectToPage("/Admin/Medicines");
    }

    private async Task LoadCategoriesAsync()
    {
        CategoryItems = await context.Categories
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.CategoryId.ToString()))
            .ToListAsync();
    }
}
