using System.ComponentModel.DataAnnotations;

namespace Pharmacy_order_system.Models;

public class User
{
    public int UserId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;

    public bool IsApproved { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
