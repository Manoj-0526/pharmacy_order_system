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
public class PrescriptionsController(PharmacyDbContext context, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet("mine")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Mine()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await context.Prescriptions.Where(x => x.UserId == userId).OrderByDescending(x => x.PrescriptionId).ToListAsync();
        return Ok(items);
    }

    [HttpGet]
    [Authorize(Roles = "Pharmacist,Admin")]
    public async Task<IActionResult> All()
    {
        var items = await context.Prescriptions.Include(x => x.User).OrderByDescending(x => x.PrescriptionId).ToListAsync();
        return Ok(items);
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!new[] { ".jpg", ".jpeg", ".png", ".pdf" }.Contains(ext))
        {
            return BadRequest("Only jpg, png, and pdf files are allowed.");
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var folder = Path.Combine(env.WebRootPath, "uploads", "prescriptions");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(folder, fileName);
        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        var prescription = new Prescription
        {
            UserId = userId,
            FilePath = $"/uploads/prescriptions/{fileName}",
            Status = PrescriptionStatus.Pending
        };

        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();

        return Ok(prescription);
    }

    [HttpPost("{id:int}/status")]
    [Authorize(Roles = "Pharmacist,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] PrescriptionStatus status)
    {
        var item = await context.Prescriptions.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = status;
        await context.SaveChangesAsync();
        return NoContent();
    }
}
