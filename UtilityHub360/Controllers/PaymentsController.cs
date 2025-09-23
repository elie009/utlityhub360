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
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> MakePayment([FromBody] CreatePaymentDto payment)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaymentDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<PaymentDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _paymentService.MakePaymentAsync(payment, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResult($"Failed to process payment: {ex.Message}"));
            }
        }

        [HttpGet("{paymentId}")]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPayment(string paymentId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaymentDto>.ErrorResult("User not authenticated"));
                }

                var result = await _paymentService.GetPaymentAsync(paymentId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResult($"Failed to get payment: {ex.Message}"));
            }
        }

        [HttpGet("loan/{loanId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaymentDto>>>> GetLoanPayments(
            string loanId,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaginatedResponse<PaymentDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _paymentService.GetLoanPaymentsAsync(loanId, userId, page, limit);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<PaymentDto>>.ErrorResult($"Failed to get loan payments: {ex.Message}"));
            }
        }

        [HttpPut("{paymentId}/status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> UpdatePaymentStatus(
            string paymentId, 
            [FromBody] UpdatePaymentStatusDto updateStatusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<PaymentDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _paymentService.UpdatePaymentStatusAsync(paymentId, updateStatusDto.Status);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaymentDto>.ErrorResult($"Failed to update payment status: {ex.Message}"));
            }
        }
    }

    public class UpdatePaymentStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
