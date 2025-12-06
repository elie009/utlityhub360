using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WhiteLabelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISubscriptionService _subscriptionService;

        public WhiteLabelController(ApplicationDbContext context, ISubscriptionService subscriptionService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
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

                // TODO: Retrieve white-label settings from database
                // This would require a WhiteLabelSettings entity
                var settings = new WhiteLabelSettingsDto
                {
                    CompanyName = "Your Company",
                    LogoUrl = null,
                    PrimaryColor = "#1976d2",
                    SecondaryColor = "#424242",
                    CustomDomain = null,
                    IsActive = false
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

                // TODO: Update white-label settings in database
                // This would require saving to a WhiteLabelSettings entity

                var settings = new WhiteLabelSettingsDto
                {
                    CompanyName = updateDto.CompanyName ?? "Your Company",
                    LogoUrl = updateDto.LogoUrl,
                    PrimaryColor = updateDto.PrimaryColor ?? "#1976d2",
                    SecondaryColor = updateDto.SecondaryColor ?? "#424242",
                    CustomDomain = updateDto.CustomDomain,
                    IsActive = updateDto.IsActive ?? false
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

                // TODO: Upload file to storage (Azure Blob, S3, or local storage)
                // For now, return a placeholder URL
                var logoUrl = $"/uploads/whitelabel/{userId}/logo{extension}";

                return Ok(ApiResponse<string>.SuccessResult(logoUrl, "Logo uploaded successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Failed to upload logo: {ex.Message}"));
            }
        }
    }
}

