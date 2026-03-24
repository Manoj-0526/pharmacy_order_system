using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Pharmacy_order_system.Pages.Staff;

[Authorize(Policy = "StaffOnly")]
public class MedicinesModel(PharmacyDbContext context) : PageModel
{
    public List<Medicine> Medicines { get; set; } = [];
    public List<SelectListItem> CategoryItems { get; set; } = [];

    [BindProperty]
    public MedicineInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

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

    public async Task<IActionResult> OnGetAsync()
    {
        var approved = await IsApprovedStaffAsync();
        if (!approved)
        {
            ErrorMessage = "Your staff account is not approved by admin yet.";
            return Page();
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!await IsApprovedStaffAsync())
        {
            return RedirectToPage();
        }

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

    public async Task<IActionResult> OnPostStockAsync(int id, int stock)
    {
        if (!await IsApprovedStaffAsync())
        {
            return RedirectToPage();
        }

        var medicine = await context.Medicines.FindAsync(id);
        if (medicine is not null)
        {
            medicine.Stock = Math.Max(0, stock);
            await context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!await IsApprovedStaffAsync())
        {
            return RedirectToPage();
        }

        var medicine = await context.Medicines.FindAsync(id);
        if (medicine is not null)
        {
            context.Medicines.Remove(medicine);
            await context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task<bool> IsApprovedStaffAsync()
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await context.Users.AnyAsync(x => x.UserId == userId && x.Role == UserRole.Staff && x.IsApproved);
    }

    private async Task LoadAsync()
    {
        Medicines = await context.Medicines.OrderBy(x => x.Name).ToListAsync();
        CategoryItems = await context.Categories
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.CategoryId.ToString()))
            .ToListAsync();
    }
}
