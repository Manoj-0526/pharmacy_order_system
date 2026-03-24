using Pharmacy_order_system.Infrastructure;
using Pharmacy_order_system.Models;

namespace Pharmacy_order_system.Services;

public class CartService(IHttpContextAccessor httpContextAccessor)
{
    private const string CartSessionKey = "CART_ITEMS";

    private ISession Session => httpContextAccessor.HttpContext!.Session;

    public List<CartItem> GetCart()
    {
        return Session.GetObject<List<CartItem>>(CartSessionKey) ?? [];
    }

    public void SaveCart(List<CartItem> items)
    {
        Session.SetObject(CartSessionKey, items);
    }

    public void AddItem(Medicine medicine, int quantity)
    {
        var cart = GetCart();
        var existing = cart.FirstOrDefault(x => x.MedicineId == medicine.MedicineId);

        if (existing is null)
        {
            cart.Add(new CartItem
            {
                MedicineId = medicine.MedicineId,
                Name = medicine.Name,
                Price = medicine.Price,
                Quantity = quantity,
                RequiresPrescription = medicine.RequiresPrescription
            });
        }
        else
        {
            existing.Quantity += quantity;
        }

        SaveCart(cart);
    }

    public void RemoveItem(int medicineId)
    {
        var cart = GetCart();
        cart.RemoveAll(x => x.MedicineId == medicineId);
        SaveCart(cart);
    }

    public void Clear()
    {
        Session.Remove(CartSessionKey);
    }
}
