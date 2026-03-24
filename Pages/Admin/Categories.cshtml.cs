using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_order_system.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class CategoriesModel(PharmacyDbContext context) : PageModel
{
    public List<Category> Categories { get; set; } = [];

    [BindProperty]
    [Required, StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

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

        if (!await context.Categories.AnyAsync(x => x.Name == CategoryName))
        {
            context.Categories.Add(new Category { Name = CategoryName.Trim() });
            await context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var category = await context.Categories.Include(x => x.Medicines).FirstOrDefaultAsync(x => x.CategoryId == id);
        if (category is not null && category.Medicines.Count == 0)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Categories = await context.Categories.Include(x => x.Medicines).OrderBy(x => x.Name).ToListAsync();
    }
}
