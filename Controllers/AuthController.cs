using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using CarDealershipAPI.Services;

namespace CarDealershipAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IOtpService _otpService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ApplicationDbContext context,
        IPasswordService passwordService,
        ITokenService tokenService,
        IOtpService otpService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _otpService = otpService;
        _logger = logger;
    }

    /// <summary>
    /// Step 1: Request registration - generates OTP
    /// </summary>
    [HttpPost("register/request")]
    public async Task<ActionResult<OtpResponseDto>> RequestRegister([FromBody] RegisterRequestDto request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "All fields are required" });
        }

        // Check if user already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        // Validate role
        if (request.Role != "Customer" && request.Role != "Admin")
        {
            return BadRequest(new { message = "Invalid role. Must be 'Customer' or 'Admin'" });
        }

        // Store registration data temporarily (in real app, use cache or temp storage)
        // For this demo, we'll generate OTP and expect client to send all data again
        var otpCode = await _otpService.GenerateOtpAsync(request.Email, "Register");

        return Ok(new OtpResponseDto
        {
            Message = "OTP sent successfully. Please verify to complete registration.",
            Email = request.Email
        });
    }

    /// <summary>
    /// Step 2: Verify OTP and complete registration
    /// </summary>
    [HttpPost("register/verify")]
    public async Task<ActionResult<AuthResponseDto>> VerifyRegister([FromBody] RegisterRequestDto request, [FromQuery] string otpCode)
    {
        // Validate OTP
        var isValidOtp = await _otpService.ValidateOtpAsync(request.Email, otpCode, "Register");
        if (!isValidOtp)
        {
            return BadRequest(new { message = "Invalid or expired OTP" });
        }

        // Check again if user exists (race condition prevention)
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role,
            Message = "Registration successful"
        });
    }

    /// <summary>
    /// Step 1: Request login - validates credentials and generates OTP
    /// </summary>
    [HttpPost("login/request")]
    public async Task<ActionResult<OtpResponseDto>> RequestLogin([FromBody] LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Generate OTP
        var otpCode = await _otpService.GenerateOtpAsync(request.Email, "Login", user.Id);

        return Ok(new OtpResponseDto
        {
            Message = "Credentials verified. OTP sent for final authentication.",
            Email = request.Email
        });
    }

    /// <summary>
    /// Step 2: Verify OTP and complete login
    /// </summary>
    [HttpPost("login/verify")]
    public async Task<ActionResult<AuthResponseDto>> VerifyLogin([FromBody] LoginOtpRequestDto request)
    {
        // Validate OTP
        var isValidOtp = await _otpService.ValidateOtpAsync(request.Email, request.OtpCode, "Login");
        if (!isValidOtp)
        {
            return BadRequest(new { message = "Invalid or expired OTP" });
        }

        // Get user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role,
            Message = "Login successful"
        });
    }
}
