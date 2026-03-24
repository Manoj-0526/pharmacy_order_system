using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;

namespace Pharmacy_order_system.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicinesController(PharmacyDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var medicines = await context.Medicines
            .Include(x => x.Category)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.MedicineId,
                x.Name,
                Category = x.Category!.Name,
                x.Dosage,
                x.Packaging,
                x.Price,
                x.Stock,
                x.RequiresPrescription
            })
            .ToListAsync();

        return Ok(medicines);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var medicine = await context.Medicines
            .Include(x => x.Category)
            .Where(x => x.MedicineId == id)
            .Select(x => new
            {
                x.MedicineId,
                x.Name,
                Category = x.Category!.Name,
                x.Dosage,
                x.Packaging,
                x.Price,
                x.Stock,
                x.RequiresPrescription
            })
            .FirstOrDefaultAsync();

        return medicine is null ? NotFound() : Ok(medicine);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] Medicine request)
    {
        var categoryExists = await context.Categories.AnyAsync(x => x.CategoryId == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest("Invalid category.");
        }

        context.Medicines.Add(request);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = request.MedicineId }, request);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] Medicine request)
    {
        var medicine = await context.Medicines.FindAsync(id);
        if (medicine is null)
        {
            return NotFound();
        }

        medicine.Name = request.Name;
        medicine.CategoryId = request.CategoryId;
        medicine.Dosage = request.Dosage;
        medicine.Packaging = request.Packaging;
        medicine.Price = request.Price;
        medicine.Stock = request.Stock;
        medicine.RequiresPrescription = request.RequiresPrescription;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var medicine = await context.Medicines.FindAsync(id);
        if (medicine is null)
        {
            return NotFound();
        }

        context.Medicines.Remove(medicine);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
