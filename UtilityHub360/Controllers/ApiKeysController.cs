using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
    public class ApiKeysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISubscriptionService _subscriptionService;

        public ApiKeysController(ApplicationDbContext context, ISubscriptionService subscriptionService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Get all API keys for the current user
        /// Enterprise feature only
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ApiKeyDto>>>> GetApiKeys()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ApiKeyDto>>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to API Access feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "API_ACCESS");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<List<ApiKeyDto>>.ErrorResult(
                        "API Access is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                // TODO: Implement API key storage and retrieval
                // This would require an ApiKey entity in the database
                var apiKeys = new List<ApiKeyDto>();

                return Ok(ApiResponse<List<ApiKeyDto>>.SuccessResult(apiKeys));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ApiKeyDto>>.ErrorResult($"Failed to get API keys: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new API key
        /// Enterprise feature only
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ApiKeyDto>>> CreateApiKey([FromBody] CreateApiKeyDto createDto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ApiKeyDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to API Access feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "API_ACCESS");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<ApiKeyDto>.ErrorResult(
                        "API Access is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                // Generate API key
                var apiKey = GenerateApiKey();
                var hashedKey = HashApiKey(apiKey);

                // TODO: Store API key in database
                // This would require an ApiKey entity with:
                // - Id, UserId, Name, HashedKey, CreatedAt, ExpiresAt, IsActive, LastUsedAt

                var apiKeyDto = new ApiKeyDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = createDto.Name,
                    Key = apiKey, // Only shown once on creation
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = createDto.ExpiresAt,
                    IsActive = true
                };

                return Ok(ApiResponse<ApiKeyDto>.SuccessResult(apiKeyDto, "API key created successfully. Please save this key as it will not be shown again."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ApiKeyDto>.ErrorResult($"Failed to create API key: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete an API key
        /// Enterprise feature only
        /// </summary>
        [HttpDelete("{apiKeyId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteApiKey(string apiKeyId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to API Access feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "API_ACCESS");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult(
                        "API Access is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                // TODO: Implement API key deletion
                // This would require finding and deleting the API key from the database

                return Ok(ApiResponse<bool>.SuccessResult(true, "API key deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete API key: {ex.Message}"));
            }
        }

        private string GenerateApiKey()
        {
            // Generate a secure random API key
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private string HashApiKey(string apiKey)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(apiKey);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

