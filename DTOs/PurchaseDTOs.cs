namespace CarDealershipAPI.DTOs;

public class PurchaseRequestDto
{
    public int VehicleId { get; set; }
}

public class PurchaseOtpDto
{
    public int VehicleId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
}

public class PurchaseDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public int VehicleYear { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ProcessSaleDto
{
    public int PurchaseId { get; set; }
    public string Status { get; set; } = "Completed"; // Completed or Cancelled
}
