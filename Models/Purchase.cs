namespace CarDealershipAPI.Models;

public class Purchase
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int VehicleId { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
}
