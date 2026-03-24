using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharmacy_order_system.Data;
using Pharmacy_order_system.Models;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_order_system.Pages.Account;

public class RegisterModel(PharmacyDbContext context) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.Role is not (UserRole.Customer or UserRole.Staff or UserRole.Pharmacist))
        {
            ModelState.AddModelError(string.Empty, "Invalid role selection.");
            return Page();
        }

        var exists = await context.Users.AnyAsync(x => x.Email == Input.Email);
        if (exists)
        {
            ModelState.AddModelError(string.Empty, "Email already registered.");
            return Page();
        }

        var user = new User
        {
            Name = Input.Name,
            Email = Input.Email,
            Role = Input.Role,
            IsApproved = Input.Role == UserRole.Customer
        };

        var hasher = new PasswordHasher<User>();
        user.Password = hasher.HashPassword(user, Input.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        TempData["Message"] = Input.Role == UserRole.Customer
            ? "Registration successful. Please login."
            : $"{Input.Role} account created. Wait for admin approval before login.";

        return RedirectToPage("/Account/Login");
    }
}
