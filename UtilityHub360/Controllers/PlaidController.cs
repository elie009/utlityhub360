using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    /// <summary>
    /// Controller for Plaid bank integration
    /// </summary>
    [ApiController]
    [Route("api/plaid")]
    [Authorize]
    public class PlaidController : ControllerBase
    {
        private readonly PlaidService _plaidService;
        private readonly IBankFeedService _bankFeedService;
        private readonly ILogger<PlaidController> _logger;
        private readonly ISubscriptionService _subscriptionService;

        public PlaidController(
            PlaidService plaidService,
            IBankFeedService bankFeedService,
            ILogger<PlaidController> logger,
            ISubscriptionService subscriptionService)
        {
            _plaidService = plaidService;
            _bankFeedService = bankFeedService;
            _logger = logger;
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Create a Link token for Plaid Link initialization
        /// </summary>
        [HttpPost("link-token")]
        public async Task<ActionResult<ApiResponse<PlaidLinkTokenResponseDto>>> CreateLinkToken([FromBody] PlaidLinkTokenRequestDto? request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PlaidLinkTokenResponseDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Bank Feed feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "BANK_FEED");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return StatusCode(403, ApiResponse<PlaidLinkTokenResponseDto>.ErrorResult(
                        "Bank Feed Integration is a Premium feature. Please upgrade to Premium to access this feature."));
                }

                var webhookUrl = request?.WebhookUrl;
                var linkToken = await _plaidService.CreateLinkTokenAsync(userId, webhookUrl);

                var response = new PlaidLinkTokenResponseDto
                {
                    LinkToken = linkToken,
                    Expiration = DateTime.UtcNow.AddHours(4) // Plaid Link tokens expire in 4 hours
                };

                return Ok(ApiResponse<PlaidLinkTokenResponseDto>.SuccessResult(response, "Link token created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Plaid Link token");
                return BadRequest(ApiResponse<PlaidLinkTokenResponseDto>.ErrorResult($"Failed to create Link token: {ex.Message}"));
            }
        }

        /// <summary>
        /// Exchange public token for access token and connect bank account
        /// </summary>
        [HttpPost("exchange-token")]
        public async Task<ActionResult<ApiResponse<string>>> ExchangePublicToken([FromBody] PlaidExchangeTokenDto exchangeDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<string>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Bank Feed feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "BANK_FEED");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return StatusCode(403, ApiResponse<string>.ErrorResult(
                        "Bank Feed Integration is a Premium feature. Please upgrade to Premium to access this feature."));
                }

                // Exchange public token for access token
                var exchangeResponse = await _plaidService.ExchangePublicTokenAsync(exchangeDto.PublicToken);
                var accessToken = exchangeResponse.AccessToken;
                var itemId = exchangeResponse.ItemId;

                // Store access token and connect the bank account
                var connectResult = await _bankFeedService.StorePlaidAccessTokenAsync(
                    userId, 
                    exchangeDto.BankAccountId, 
                    accessToken, 
                    itemId);

                if (!connectResult.Success)
                {
                    return BadRequest(connectResult);
                }

                return Ok(ApiResponse<string>.SuccessResult(accessToken, "Account connected successfully via Plaid"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging Plaid public token");
                return BadRequest(ApiResponse<string>.ErrorResult($"Failed to exchange token: {ex.Message}"));
            }
        }
    }
}

