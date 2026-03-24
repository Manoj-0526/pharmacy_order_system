using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_order_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class MedicinesModel(PharmacyDbContext context) : PageModel
{
    public List<Medicine> Medicines { get; set; } = [];
    public List<SelectListItem> CategoryItems { get; set; } = [];

    [BindProperty]
    public MedicineInput Input { get; set; } = new();

    public class MedicineInput
    {
        [Required] public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        [Required] public string Dosage { get; set; } = string.Empty;
        [Required] public string Packaging { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool RequiresPrescription { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        context.Medicines.Add(new Medicine
        {
            Name = Input.Name,
            CategoryId = Input.CategoryId,
            Dosage = Input.Dosage,
            Packaging = Input.Packaging,
            Price = Input.Price,
            Stock = Input.Stock,
            RequiresPrescription = Input.RequiresPrescription
        });

        await context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var medicine = await context.Medicines.FindAsync(id);
        if (medicine is not null)
        {
            context.Medicines.Remove(medicine);
            await context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Medicines = await context.Medicines.Include(x => x.Category).OrderBy(x => x.Name).ToListAsync();
        CategoryItems = await context.Categories
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.CategoryId.ToString()))
            .ToListAsync();
    }
}
