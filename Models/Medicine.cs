using System.ComponentModel.DataAnnotations;

namespace Pharmacy_order_system.Models;

public class Medicine
{
    public int MedicineId { get; set; }

    [Required, StringLength(180)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [Required, StringLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Packaging { get; set; } = string.Empty;

    [Range(0, 999999)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool RequiresPrescription { get; set; }

    public Category? Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
