namespace CarDealershipAPI.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string VIN { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CreateVehicleDto
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string VIN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateVehicleRequestDto
{
    public int VehicleId { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public decimal? Price { get; set; }
    public string? Color { get; set; }
    public int? Mileage { get; set; }
    public bool? IsAvailable { get; set; }
    public string? Description { get; set; }
}

public class UpdateVehicleOtpDto
{
    public int VehicleId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
    public UpdateVehicleRequestDto UpdateData { get; set; } = null!;
}
