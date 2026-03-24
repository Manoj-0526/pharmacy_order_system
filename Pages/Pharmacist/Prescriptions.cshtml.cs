using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;

namespace Pharmacy_order_system.Pages.Pharmacist;

[Authorize(Policy = "PharmacistOnly")]
public class PrescriptionsModel(PharmacyDbContext context) : PageModel
{
    public List<Prescription> Prescriptions { get; set; } = [];

    public async Task OnGetAsync()
    {
        Prescriptions = await context.Prescriptions
            .Include(x => x.User)
            .OrderByDescending(x => x.PrescriptionId)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, PrescriptionStatus status)
    {
        var prescription = await context.Prescriptions.FindAsync(id);
        if (prescription is null)
        {
            return RedirectToPage();
        }

        prescription.Status = status;
        await context.SaveChangesAsync();

        return RedirectToPage();
    }
}
