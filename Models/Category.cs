using System.ComponentModel.DataAnnotations;

namespace Pharmacy_order_system.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}
