using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using CarDealershipAPI.Services;
using System.Security.Claims;

namespace CarDealershipAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly ILogger<PurchasesController> _logger;

    public PurchasesController(
        ApplicationDbContext context,
        IOtpService otpService,
        ILogger<PurchasesController> logger)
    {
        _context = context;
        _otpService = otpService;
        _logger = logger;
    }

    /// <summary>
    /// Step 1: Request purchase - generates OTP (Customer)
    /// </summary>
    [HttpPost("request")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OtpResponseDto>> RequestPurchase([FromBody] PurchaseRequestDto request)
    {
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        if (!vehicle.IsAvailable)
        {
            return BadRequest(new { message = "Vehicle is not available for purchase" });
        }

        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "User email not found" });
        }

        // Generate OTP
        var otpCode = await _otpService.GenerateOtpAsync(userEmail, "PurchaseRequest");

        return Ok(new OtpResponseDto
        {
            Message = "OTP sent successfully. Please verify to complete purchase request.",
            Email = userEmail
        });
    }

    /// <summary>
    /// Step 2: Verify OTP and create purchase request (Customer)
    /// </summary>
    [HttpPost("verify")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<PurchaseDto>> VerifyPurchase(
        [FromBody] PurchaseRequestDto request,
        [FromQuery] string otpCode)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "User email not found" });
        }

        // Validate OTP
        var isValidOtp = await _otpService.ValidateOtpAsync(userEmail, otpCode, "PurchaseRequest");
        if (!isValidOtp)
        {
            return BadRequest(new { message = "Invalid or expired OTP" });
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);

        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        if (!vehicle.IsAvailable)
        {
            return BadRequest(new { message = "Vehicle is not available for purchase" });
        }

        // Create purchase
        var purchase = new Purchase
        {
            UserId = userId,
            VehicleId = request.VehicleId,
            PurchasePrice = vehicle.Price,
            PurchaseDate = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        // Load navigation properties
        await _context.Entry(purchase).Reference(p => p.Vehicle).LoadAsync();

        var purchaseDto = new PurchaseDto
        {
            Id = purchase.Id,
            VehicleId = purchase.VehicleId,
            VehicleMake = purchase.Vehicle.Make,
            VehicleModel = purchase.Vehicle.Model,
            VehicleYear = purchase.Vehicle.Year,
            PurchasePrice = purchase.PurchasePrice,
            PurchaseDate = purchase.PurchaseDate,
            Status = purchase.Status
        };

        return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, purchaseDto);
    }

    /// <summary>
    /// Get purchase history for the current user (Customer)
    /// </summary>
    [HttpGet("my-purchases")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetMyPurchases()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var purchases = await _context.Purchases
            .Include(p => p.Vehicle)
            .Where(p => p.UserId == userId)
            .Select(p => new PurchaseDto
            {
                Id = p.Id,
                VehicleId = p.VehicleId,
                VehicleMake = p.Vehicle.Make,
                VehicleModel = p.Vehicle.Model,
                VehicleYear = p.Vehicle.Year,
                PurchasePrice = p.PurchasePrice,
                PurchaseDate = p.PurchaseDate,
                Status = p.Status
            })
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return Ok(purchases);
    }

    /// <summary>
    /// Get a specific purchase by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseDto>> GetPurchase(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var purchase = await _context.Purchases
            .Include(p => p.Vehicle)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null)
        {
            return NotFound(new { message = "Purchase not found" });
        }

        // Customers can only see their own purchases
        if (userRole == "Customer" && purchase.UserId != userId)
        {
            return Forbid();
        }

        var purchaseDto = new PurchaseDto
        {
            Id = purchase.Id,
            VehicleId = purchase.VehicleId,
            VehicleMake = purchase.Vehicle.Make,
            VehicleModel = purchase.Vehicle.Model,
            VehicleYear = purchase.Vehicle.Year,
            PurchasePrice = purchase.PurchasePrice,
            PurchaseDate = purchase.PurchaseDate,
            Status = purchase.Status
        };

        return Ok(purchaseDto);
    }

    /// <summary>
    /// Get all purchases (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetAllPurchases()
    {
        var purchases = await _context.Purchases
            .Include(p => p.Vehicle)
            .Select(p => new PurchaseDto
            {
                Id = p.Id,
                VehicleId = p.VehicleId,
                VehicleMake = p.Vehicle.Make,
                VehicleModel = p.Vehicle.Model,
                VehicleYear = p.Vehicle.Year,
                PurchasePrice = p.PurchasePrice,
                PurchaseDate = p.PurchaseDate,
                Status = p.Status
            })
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return Ok(purchases);
    }

    /// <summary>
    /// Process a sale - complete or cancel (Admin only)
    /// </summary>
    [HttpPut("{id}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PurchaseDto>> ProcessSale(int id, [FromBody] ProcessSaleDto request)
    {
        var purchase = await _context.Purchases
            .Include(p => p.Vehicle)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null)
        {
            return NotFound(new { message = "Purchase not found" });
        }

        if (purchase.Status != "Pending")
        {
            return BadRequest(new { message = $"Purchase is already {purchase.Status}" });
        }

        if (request.Status != "Completed" && request.Status != "Cancelled")
        {
            return BadRequest(new { message = "Status must be 'Completed' or 'Cancelled'" });
        }

        purchase.Status = request.Status;

        // If completed, mark vehicle as unavailable
        if (request.Status == "Completed")
        {
            purchase.Vehicle.IsAvailable = false;
        }

        await _context.SaveChangesAsync();

        var purchaseDto = new PurchaseDto
        {
            Id = purchase.Id,
            VehicleId = purchase.VehicleId,
            VehicleMake = purchase.Vehicle.Make,
            VehicleModel = purchase.Vehicle.Model,
            VehicleYear = purchase.Vehicle.Year,
            PurchasePrice = purchase.PurchasePrice,
            PurchaseDate = purchase.PurchaseDate,
            Status = purchase.Status
        };

        return Ok(purchaseDto);
    }
}
