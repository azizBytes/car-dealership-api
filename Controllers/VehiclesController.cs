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
public class VehiclesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(
        ApplicationDbContext context,
        IOtpService otpService,
        ILogger<VehiclesController> logger)
    {
        _context = context;
        _otpService = otpService;
        _logger = logger;
    }

    /// <summary>
    /// Browse all available vehicles with optional filtering
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles(
        [FromQuery] string? make,
        [FromQuery] string? model,
        [FromQuery] int? minYear,
        [FromQuery] int? maxYear,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? isAvailable)
    {
        var query = _context.Vehicles.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(make))
            query = query.Where(v => v.Make.Contains(make));

        if (!string.IsNullOrWhiteSpace(model))
            query = query.Where(v => v.Model.Contains(model));

        if (minYear.HasValue)
            query = query.Where(v => v.Year >= minYear.Value);

        if (maxYear.HasValue)
            query = query.Where(v => v.Year <= maxYear.Value);

        if (minPrice.HasValue)
            query = query.Where(v => v.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(v => v.Price <= maxPrice.Value);

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var vehicles = await query
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                Price = v.Price,
                Color = v.Color,
                Mileage = v.Mileage,
                VIN = v.VIN,
                IsAvailable = v.IsAvailable,
                Description = v.Description
            })
            .ToListAsync();

        return Ok(vehicles);
    }

    /// <summary>
    /// Get detailed information about a specific vehicle
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<VehicleDto>> GetVehicle(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        var vehicleDto = new VehicleDto
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            Color = vehicle.Color,
            Mileage = vehicle.Mileage,
            VIN = vehicle.VIN,
            IsAvailable = vehicle.IsAvailable,
            Description = vehicle.Description
        };

        return Ok(vehicleDto);
    }

    /// <summary>
    /// Add a new vehicle to inventory (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehicleDto>> AddVehicle([FromBody] CreateVehicleDto request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Make) || 
            string.IsNullOrWhiteSpace(request.Model) ||
            string.IsNullOrWhiteSpace(request.VIN))
        {
            return BadRequest(new { message = "Make, Model, and VIN are required" });
        }

        if (request.Year < 1900 || request.Year > DateTime.Now.Year + 1)
        {
            return BadRequest(new { message = "Invalid year" });
        }

        if (request.Price <= 0)
        {
            return BadRequest(new { message = "Price must be greater than zero" });
        }

        // Check if VIN already exists
        var existingVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VIN == request.VIN);
        if (existingVehicle != null)
        {
            return BadRequest(new { message = "Vehicle with this VIN already exists" });
        }

        var vehicle = new Vehicle
        {
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Price = request.Price,
            Color = request.Color,
            Mileage = request.Mileage,
            VIN = request.VIN,
            Description = request.Description,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var vehicleDto = new VehicleDto
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            Color = vehicle.Color,
            Mileage = vehicle.Mileage,
            VIN = vehicle.VIN,
            IsAvailable = vehicle.IsAvailable,
            Description = vehicle.Description
        };

        return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicleDto);
    }

    /// <summary>
    /// Step 1: Request vehicle update - generates OTP (Admin only)
    /// </summary>
    [HttpPost("update/request")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OtpResponseDto>> RequestUpdateVehicle([FromBody] UpdateVehicleRequestDto request)
    {
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "User email not found" });
        }

        // Generate OTP
        var otpCode = await _otpService.GenerateOtpAsync(userEmail, "UpdateVehicle");

        return Ok(new OtpResponseDto
        {
            Message = "OTP sent successfully. Please verify to complete vehicle update.",
            Email = userEmail
        });
    }

    /// <summary>
    /// Step 2: Verify OTP and update vehicle (Admin only)
    /// </summary>
    [HttpPut("update/verify")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehicleDto>> VerifyUpdateVehicle(
        [FromBody] UpdateVehicleRequestDto request,
        [FromQuery] string otpCode)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "User email not found" });
        }

        // Validate OTP
        var isValidOtp = await _otpService.ValidateOtpAsync(userEmail, otpCode, "UpdateVehicle");
        if (!isValidOtp)
        {
            return BadRequest(new { message = "Invalid or expired OTP" });
        }

        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.Make))
            vehicle.Make = request.Make;

        if (!string.IsNullOrWhiteSpace(request.Model))
            vehicle.Model = request.Model;

        if (request.Year.HasValue && request.Year.Value >= 1900 && request.Year.Value <= DateTime.Now.Year + 1)
            vehicle.Year = request.Year.Value;

        if (request.Price.HasValue && request.Price.Value > 0)
            vehicle.Price = request.Price.Value;

        if (!string.IsNullOrWhiteSpace(request.Color))
            vehicle.Color = request.Color;

        if (request.Mileage.HasValue && request.Mileage.Value >= 0)
            vehicle.Mileage = request.Mileage.Value;

        if (request.IsAvailable.HasValue)
            vehicle.IsAvailable = request.IsAvailable.Value;

        if (!string.IsNullOrWhiteSpace(request.Description))
            vehicle.Description = request.Description;

        await _context.SaveChangesAsync();

        var vehicleDto = new VehicleDto
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            Color = vehicle.Color,
            Mileage = vehicle.Mileage,
            VIN = vehicle.VIN,
            IsAvailable = vehicle.IsAvailable,
            Description = vehicle.Description
        };

        return Ok(vehicleDto);
    }

    /// <summary>
    /// Delete a vehicle (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Vehicle deleted successfully" });
    }
}
