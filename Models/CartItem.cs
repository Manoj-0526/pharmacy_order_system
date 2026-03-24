namespace Pharmacy_order_system.Models;

public class CartItem
{
    public int MedicineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool RequiresPrescription { get; set; }
    public decimal LineTotal => Price * Quantity;
}
