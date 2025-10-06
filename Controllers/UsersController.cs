using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarDealershipAPI.Data;

namespace CarDealershipAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all customers (Admin only)
    /// </summary>
    [HttpGet("customers")]
    public async Task<ActionResult> GetAllCustomers()
    {
        var customers = await _context.Users
            .Where(u => u.Role == "Customer")
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.CreatedAt,
                PurchaseCount = u.Purchases.Count
            })
            .ToListAsync();

        return Ok(customers);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt,
                PurchaseCount = u.Purchases.Count
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt,
                Purchases = u.Purchases.Select(p => new
                {
                    p.Id,
                    p.VehicleId,
                    p.PurchasePrice,
                    p.PurchaseDate,
                    p.Status
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }
}
