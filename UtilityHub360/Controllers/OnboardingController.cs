using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OnboardingController : ControllerBase
    {
        private readonly IOnboardingService _onboardingService;

        public OnboardingController(IOnboardingService onboardingService)
        {
            _onboardingService = onboardingService;
        }

        /// <summary>
        /// Get the current user's onboarding progress
        /// </summary>
        [HttpGet("progress")]
        public async Task<ActionResult<ApiResponse<OnboardingProgressDto>>> GetOnboardingProgress()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OnboardingProgressDto>.ErrorResult("User not authenticated"));
                }

                var result = await _onboardingService.GetOnboardingProgressAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to get onboarding progress: {ex.Message}"));
            }
        }


        /// <summary>
        /// Start the onboarding process
        /// </summary>
        [HttpPost("start")]
        public async Task<ActionResult<ApiResponse<OnboardingProgressDto>>> StartOnboarding()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OnboardingProgressDto>.ErrorResult("User not authenticated"));
                }

                var result = await _onboardingService.StartOnboardingAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to start onboarding: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update the current step in the onboarding process
        /// </summary>
        [HttpPut("current-step/{stepNumber}")]
        public async Task<ActionResult<ApiResponse<OnboardingProgressDto>>> UpdateCurrentStep(int stepNumber)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OnboardingProgressDto>.ErrorResult("User not authenticated"));
                }

                var result = await _onboardingService.UpdateCurrentStepAsync(userId, stepNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to update current step: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete the welcome and preferences setup step
        /// </summary>
        [HttpPost("complete-welcome")]
        public async Task<ActionResult<ApiResponse<QuickSetupResponseDto>>> CompleteWelcomeStep([FromBody] WelcomeSetupDto welcomeSetup)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<QuickSetupResponseDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _onboardingService.CompleteWelcomeStepAsync(userId, welcomeSetup);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete welcome step: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete the income sources setup step
        /// </summary>
        [HttpPost("complete-income")]
        public async Task<ActionResult<ApiResponse<QuickSetupResponseDto>>> CompleteIncomeStep([FromBody] IncomeSetupDto incomeSetup)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<QuickSetupResponseDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _onboardingService.CompleteIncomeStepAsync(userId, incomeSetup);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete income step: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete the bills setup step
        /// </summary>
        [HttpPost("complete-bills")]
        public async Task<ActionResult<ApiResponse<QuickSetupResponseDto>>> CompleteBillsStep([FromBody] BillsSetupDto billsSetup)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<QuickSetupResponseDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _onboardingService.CompleteBillsStepAsync(userId, billsSetup);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete bills step: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete the loans setup step
        /// </summary>
        [HttpPost("complete-loans")]
        public async Task<ActionResult<ApiResponse<QuickSetupResponseDto>>> CompleteLoansStep([FromBody] LoansSetupDto loansSetup)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<QuickSetupResponseDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _onboardingService.CompleteLoansStepAsync(userId, loansSetup);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete loans step: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete the variable expenses setup step
        /// </summary>
        [HttpPost("complete-expenses")]
        public async Task<ActionResult<ApiResponse<QuickSetupResponseDto>>> CompleteVariableExpensesStep([FromBody] VariableExpensesSetupDto expensesSetup)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<QuickSetupResponseDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _onboardingService.CompleteVariableExpensesStepAsync(userId, expensesSetup);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<QuickSetupResponseDto>.ErrorResult($"Failed to complete variable expenses step: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete the dashboard tour step
        /// </summary>
        [HttpPost("complete-tour")]
        public async Task<ActionResult<ApiResponse<OnboardingProgressDto>>> CompleteDashboardTour([FromBody] DashboardTourDto dashboardTour)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OnboardingProgressDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _onboardingService.CompleteDashboardTourAsync(userId, dashboardTour);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to complete dashboard tour: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete the entire onboarding process in one request
        /// </summary>
        [HttpPost("complete-all")]
        public async Task<ActionResult<ApiResponse<OnboardingProgressDto>>> CompleteOnboarding([FromBody] CompleteOnboardingDto completeOnboarding)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OnboardingProgressDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _onboardingService.CompleteOnboardingAsync(userId, completeOnboarding);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to complete onboarding: {ex.Message}"));
            }
        }

        /// <summary>
        /// Skip the onboarding process entirely
        /// </summary>
        [HttpPost("skip")]
        public async Task<ActionResult<ApiResponse<OnboardingProgressDto>>> SkipOnboarding()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OnboardingProgressDto>.ErrorResult("User not authenticated"));
                }

                var result = await _onboardingService.SkipOnboardingAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to skip onboarding: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reset the onboarding process to start over
        /// </summary>
        [HttpPost("reset")]
        public async Task<ActionResult<ApiResponse<OnboardingProgressDto>>> ResetOnboarding()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OnboardingProgressDto>.ErrorResult("User not authenticated"));
                }

                var result = await _onboardingService.ResetOnboardingAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<OnboardingProgressDto>.ErrorResult($"Failed to reset onboarding: {ex.Message}"));
            }
        }


    }
}
