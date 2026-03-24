using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Pharmacy_order_system.Pages.Prescriptions;

[Authorize(Policy = "CustomerOnly")]
public class UploadModel(PharmacyDbContext context, IWebHostEnvironment webHostEnvironment) : PageModel
{
    [BindProperty]
    [Required]
    public IFormFile? PrescriptionFile { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool FromCart { get; set; }

    public List<Prescription> Prescriptions { get; set; } = [];

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        if (FromCart && string.IsNullOrWhiteSpace(Message))
        {
            Message = "Prescription medicine is in your cart. Upload prescription to continue.";
        }

        await LoadPrescriptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || PrescriptionFile is null)
        {
            await LoadPrescriptionsAsync();
            return Page();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var extension = Path.GetExtension(PrescriptionFile.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        if (!allowed.Contains(extension))
        {
            ModelState.AddModelError(string.Empty, "Only jpg, png, and pdf files are allowed.");
            await LoadPrescriptionsAsync();
            return Page();
        }

        var uploadFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "prescriptions");
        Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await PrescriptionFile.CopyToAsync(stream);
        }

        context.Prescriptions.Add(new Prescription
        {
            UserId = userId,
            FilePath = $"/uploads/prescriptions/{fileName}",
            Status = PrescriptionStatus.Pending
        });

        await context.SaveChangesAsync();

        if (FromCart)
        {
            TempData["CartMessage"] = "Prescription uploaded. Once approved by pharmacist, you can checkout from cart.";
            return RedirectToPage("/Cart/Index");
        }

        Message = "Prescription uploaded and sent for pharmacist review.";
        return RedirectToPage();
    }

    private async Task LoadPrescriptionsAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        Prescriptions = await context.Prescriptions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.PrescriptionId)
            .ToListAsync();
    }
}
