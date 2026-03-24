using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Models;

namespace Pharmacy_order_system.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
        var hasher = new PasswordHasher<User>();

        await context.Database.EnsureCreatedAsync();

        if (!await context.Users.AnyAsync())
        {
            var admin = new User
            {
                Name = "System Admin",
                Email = "admin@pharmacy.com",
                Role = UserRole.Admin,
                IsApproved = true
            };
            admin.Password = hasher.HashPassword(admin, "Admin@123");

            var pharmacist = new User
            {
                Name = "Main Pharmacist",
                Email = "pharmacist@pharmacy.com",
                Role = UserRole.Pharmacist,
                IsApproved = true
            };
            pharmacist.Password = hasher.HashPassword(pharmacist, "Pharma@123");

            var staff = new User
            {
                Name = "Store Staff",
                Email = "staff@pharmacy.com",
                Role = UserRole.Staff,
                IsApproved = true
            };
            staff.Password = hasher.HashPassword(staff, "Staff@123");

            var customer = new User
            {
                Name = "Demo Customer",
                Email = "customer@pharmacy.com",
                Role = UserRole.Customer,
                IsApproved = true
            };
            customer.Password = hasher.HashPassword(customer, "Customer@123");

            await context.Users.AddRangeAsync(admin, pharmacist, staff, customer);
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "Pain Relief" },
                new Category { Name = "Cold & Flu" },
                new Category { Name = "Diabetes" },
                new Category { Name = "Heart Care" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        if (!await context.Medicines.AnyAsync())
        {
            var painReliefId = await context.Categories.Where(x => x.Name == "Pain Relief").Select(x => x.CategoryId).FirstAsync();
            var coldFluId = await context.Categories.Where(x => x.Name == "Cold & Flu").Select(x => x.CategoryId).FirstAsync();
            var diabetesId = await context.Categories.Where(x => x.Name == "Diabetes").Select(x => x.CategoryId).FirstAsync();

            var medicines = new[]
            {
                new Medicine { Name = "Paracetamol 500mg", CategoryId = painReliefId, Dosage = "500mg", Packaging = "10 tablets", Price = 45, Stock = 250, RequiresPrescription = false },
                new Medicine { Name = "Ibuprofen 400mg", CategoryId = painReliefId, Dosage = "400mg", Packaging = "10 tablets", Price = 65, Stock = 190, RequiresPrescription = false },
                new Medicine { Name = "Azithromycin 500mg", CategoryId = coldFluId, Dosage = "500mg", Packaging = "6 tablets", Price = 210, Stock = 90, RequiresPrescription = true },
                new Medicine { Name = "Metformin 500mg", CategoryId = diabetesId, Dosage = "500mg", Packaging = "15 tablets", Price = 135, Stock = 120, RequiresPrescription = true }
            };

            await context.Medicines.AddRangeAsync(medicines);
        }

        await context.SaveChangesAsync();
    }
}
