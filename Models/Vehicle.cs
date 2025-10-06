namespace CarDealershipAPI.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string VIN { get; set; } = string.Empty; // Vehicle Identification Number
    public bool IsAvailable { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
