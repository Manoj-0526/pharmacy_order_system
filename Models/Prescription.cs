using System.ComponentModel.DataAnnotations;

namespace Pharmacy_order_system.Models;

public class Prescription
{
    public int PrescriptionId { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(300)]
    public string FilePath { get; set; } = string.Empty;

    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;

    public User? User { get; set; }
}
