using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.IO;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;
using UtilityHub360.Services;
using Microsoft.AspNetCore.Hosting;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WhiteLabelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<WhiteLabelController> _logger;

        public WhiteLabelController(
            ApplicationDbContext context, 
            ISubscriptionService subscriptionService,
            IWebHostEnvironment environment,
            ILogger<WhiteLabelController> logger)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _environment = environment;
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Get white-label branding settings
        /// Enterprise feature only
        /// </summary>
        [HttpGet("settings")]
        public async Task<ActionResult<ApiResponse<WhiteLabelSettingsDto>>> GetWhiteLabelSettings()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<WhiteLabelSettingsDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to White-Label feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "WHITE_LABEL");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<WhiteLabelSettingsDto>.ErrorResult(
                        "White-Label is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                // Retrieve white-label settings from database
                var whiteLabelSettings = await _context.WhiteLabelSettings
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (whiteLabelSettings == null)
                {
                    // Return default settings if none exist
                    var defaultSettings = new WhiteLabelSettingsDto
                    {
                        CompanyName = "Your Company",
                        LogoUrl = null,
                        PrimaryColor = "#1976d2",
                        SecondaryColor = "#424242",
                        CustomDomain = null,
                        IsActive = false
                    };
                    return Ok(ApiResponse<WhiteLabelSettingsDto>.SuccessResult(defaultSettings));
                }

                var settings = new WhiteLabelSettingsDto
                {
                    CompanyName = whiteLabelSettings.CompanyName,
                    LogoUrl = whiteLabelSettings.LogoUrl,
                    PrimaryColor = whiteLabelSettings.PrimaryColor,
                    SecondaryColor = whiteLabelSettings.SecondaryColor,
                    CustomDomain = whiteLabelSettings.CustomDomain,
                    IsActive = whiteLabelSettings.IsActive
                };

                return Ok(ApiResponse<WhiteLabelSettingsDto>.SuccessResult(settings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WhiteLabelSettingsDto>.ErrorResult($"Failed to get white-label settings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update white-label branding settings
        /// Enterprise feature only
        /// </summary>
        [HttpPut("settings")]
        public async Task<ActionResult<ApiResponse<WhiteLabelSettingsDto>>> UpdateWhiteLabelSettings([FromBody] UpdateWhiteLabelSettingsDto updateDto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<WhiteLabelSettingsDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to White-Label feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "WHITE_LABEL");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<WhiteLabelSettingsDto>.ErrorResult(
                        "White-Label is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                // Get or create white-label settings
                var whiteLabelSettings = await _context.WhiteLabelSettings
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (whiteLabelSettings == null)
                {
                    whiteLabelSettings = new WhiteLabelSettings
                    {
                        UserId = userId,
                        CompanyName = updateDto.CompanyName ?? "Your Company",
                        LogoUrl = updateDto.LogoUrl,
                        PrimaryColor = updateDto.PrimaryColor ?? "#1976d2",
                        SecondaryColor = updateDto.SecondaryColor ?? "#424242",
                        CustomDomain = updateDto.CustomDomain,
                        IsActive = updateDto.IsActive ?? false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.WhiteLabelSettings.Add(whiteLabelSettings);
                }
                else
                {
                    // Update existing settings
                    if (!string.IsNullOrEmpty(updateDto.CompanyName))
                        whiteLabelSettings.CompanyName = updateDto.CompanyName;
                    if (updateDto.LogoUrl != null)
                        whiteLabelSettings.LogoUrl = updateDto.LogoUrl;
                    if (!string.IsNullOrEmpty(updateDto.PrimaryColor))
                        whiteLabelSettings.PrimaryColor = updateDto.PrimaryColor;
                    if (!string.IsNullOrEmpty(updateDto.SecondaryColor))
                        whiteLabelSettings.SecondaryColor = updateDto.SecondaryColor;
                    if (updateDto.CustomDomain != null)
                        whiteLabelSettings.CustomDomain = updateDto.CustomDomain;
                    if (updateDto.IsActive.HasValue)
                        whiteLabelSettings.IsActive = updateDto.IsActive.Value;
                    whiteLabelSettings.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                var settings = new WhiteLabelSettingsDto
                {
                    CompanyName = whiteLabelSettings.CompanyName,
                    LogoUrl = whiteLabelSettings.LogoUrl,
                    PrimaryColor = whiteLabelSettings.PrimaryColor,
                    SecondaryColor = whiteLabelSettings.SecondaryColor,
                    CustomDomain = whiteLabelSettings.CustomDomain,
                    IsActive = whiteLabelSettings.IsActive
                };

                return Ok(ApiResponse<WhiteLabelSettingsDto>.SuccessResult(settings, "White-label settings updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WhiteLabelSettingsDto>.ErrorResult($"Failed to update white-label settings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Upload logo for white-label branding
        /// Enterprise feature only
        /// </summary>
        [HttpPost("logo")]
        public async Task<ActionResult<ApiResponse<string>>> UploadLogo([FromForm] IFormFile logoFile)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<string>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to White-Label feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "WHITE_LABEL");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult(
                        "White-Label is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                if (logoFile == null || logoFile.Length == 0)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("No file uploaded"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
                var extension = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("Invalid file type. Only PNG, JPG, JPEG, and SVG files are allowed."));
                }

                // Validate file size (max 5MB)
                var maxFileSize = 5 * 1024 * 1024; // 5MB
                if (logoFile.Length > maxFileSize)
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("File size exceeds maximum limit of 5MB."));
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "whitelabel", userId);
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"logo_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);
                var relativePath = Path.Combine("uploads", "whitelabel", userId, fileName).Replace("\\", "/");

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                // Update white-label settings with logo URL
                var whiteLabelSettings = await _context.WhiteLabelSettings
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (whiteLabelSettings != null)
                {
                    // Delete old logo if exists
                    if (!string.IsNullOrEmpty(whiteLabelSettings.LogoUrl))
                    {
                        var oldLogoPath = Path.Combine(_environment.ContentRootPath, whiteLabelSettings.LogoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldLogoPath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldLogoPath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete old logo file: {Path}", oldLogoPath);
                            }
                        }
                    }

                    whiteLabelSettings.LogoUrl = $"/{relativePath}";
                    whiteLabelSettings.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Ok(ApiResponse<string>.SuccessResult($"/{relativePath}", "Logo uploaded successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Failed to upload logo: {ex.Message}"));
            }
        }
    }
}

