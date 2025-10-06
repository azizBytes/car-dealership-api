using CarDealershipAPI.Data;
using CarDealershipAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CarDealershipAPI.Services;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string email, string purpose, int? userId = null);
    Task<bool> ValidateOtpAsync(string email, string code, string purpose);
    Task CleanupExpiredOtpsAsync();
}

public class OtpService : IOtpService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OtpService> _logger;
    private const int OTP_LENGTH = 6;
    private const int OTP_EXPIRY_MINUTES = 5;

    public OtpService(ApplicationDbContext context, ILogger<OtpService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateOtpAsync(string email, string purpose, int? userId = null)
    {
        // Generate random 6-digit OTP
        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        var otp = new OtpCode
        {
            Email = email,
            Code = otpCode,
            Purpose = purpose,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
            IsUsed = false,
            UserId = userId
        };

        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        // Simulate OTP delivery (console output)
        _logger.LogInformation($"[OTP DELIVERY SIMULATION]");
        _logger.LogInformation($"To: {email}");
        _logger.LogInformation($"Purpose: {purpose}");
        _logger.LogInformation($"OTP Code: {otpCode}");
        _logger.LogInformation($"Expires at: {otp.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");
        _logger.LogInformation($"Valid for: {OTP_EXPIRY_MINUTES} minutes");
        _logger.LogInformation("=====================================");

        return otpCode;
    }

    public async Task<bool> ValidateOtpAsync(string email, string code, string purpose)
    {
        var otp = await _context.OtpCodes
            .Where(o => o.Email == email && 
                       o.Code == code && 
                       o.Purpose == purpose && 
                       !o.IsUsed && 
                       o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return false;
        }

        // Mark OTP as used
        otp.IsUsed = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task CleanupExpiredOtpsAsync()
    {
        var expiredOtps = await _context.OtpCodes
            .Where(o => o.ExpiresAt < DateTime.UtcNow || o.IsUsed)
            .ToListAsync();

        _context.OtpCodes.RemoveRange(expiredOtps);
        await _context.SaveChangesAsync();
    }
}
