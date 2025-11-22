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
    public class ReceivablesController : ControllerBase
    {
        private readonly IReceivableService _receivableService;

        public ReceivablesController(IReceivableService receivableService)
        {
            _receivableService = receivableService;
        }

        /// <summary>
        /// Create a new receivable (money lent to someone)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReceivableDto>>> CreateReceivable([FromBody] CreateReceivableDto createReceivableDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceivableDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ReceivableDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _receivableService.CreateReceivableAsync(createReceivableDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReceivableDto>.ErrorResult($"Failed to create receivable: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific receivable by ID
        /// </summary>
        [HttpGet("{receivableId}")]
        public async Task<ActionResult<ApiResponse<ReceivableDto>>> GetReceivable(string receivableId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceivableDto>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.GetReceivableAsync(receivableId, userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReceivableDto>.ErrorResult($"Failed to get receivable: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all receivables for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ReceivableDto>>>> GetReceivables([FromQuery] string? status = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ReceivableDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.GetUserReceivablesAsync(userId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ReceivableDto>>.ErrorResult($"Failed to get receivables: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update a receivable
        /// </summary>
        [HttpPut("{receivableId}")]
        public async Task<ActionResult<ApiResponse<ReceivableDto>>> UpdateReceivable(string receivableId, [FromBody] UpdateReceivableDto updateReceivableDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceivableDto>.ErrorResult("User not authenticated"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ReceivableDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _receivableService.UpdateReceivableAsync(receivableId, updateReceivableDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReceivableDto>.ErrorResult($"Failed to update receivable: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a receivable
        /// </summary>
        [HttpDelete("{receivableId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReceivable(string receivableId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.DeleteReceivableAsync(receivableId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete receivable: {ex.Message}"));
            }
        }

        /// <summary>
        /// Record a payment received for a receivable
        /// </summary>
        [HttpPost("{receivableId}/payments")]
        public async Task<ActionResult<ApiResponse<ReceivablePaymentDto>>> RecordPayment(string receivableId, [FromBody] CreateReceivablePaymentDto paymentDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceivablePaymentDto>.ErrorResult("User not authenticated"));
                }

                // Set the receivable ID from the route
                paymentDto.ReceivableId = receivableId;

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ReceivablePaymentDto>.ErrorResult("Validation failed", errors));
                }

                var result = await _receivableService.RecordPaymentAsync(paymentDto, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReceivablePaymentDto>.ErrorResult($"Failed to record payment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all payments for a receivable
        /// </summary>
        [HttpGet("{receivableId}/payments")]
        public async Task<ActionResult<ApiResponse<List<ReceivablePaymentDto>>>> GetReceivablePayments(string receivableId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<ReceivablePaymentDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.GetReceivablePaymentsAsync(receivableId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ReceivablePaymentDto>>.ErrorResult($"Failed to get payments: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific payment by ID
        /// </summary>
        [HttpGet("payments/{paymentId}")]
        public async Task<ActionResult<ApiResponse<ReceivablePaymentDto>>> GetPayment(string paymentId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceivablePaymentDto>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.GetPaymentAsync(paymentId, userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReceivablePaymentDto>.ErrorResult($"Failed to get payment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a payment
        /// </summary>
        [HttpDelete("payments/{paymentId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePayment(string paymentId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.DeletePaymentAsync(paymentId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete payment: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get total receivables amount
        /// </summary>
        [HttpGet("summary/total")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalReceivables()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.GetTotalReceivablesAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total receivables: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get total outstanding amount (money still owed)
        /// </summary>
        [HttpGet("summary/outstanding")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalOutstanding()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.GetTotalOutstandingAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total outstanding: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get total paid amount
        /// </summary>
        [HttpGet("summary/paid")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalPaid()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<decimal>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.GetTotalPaidAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<decimal>.ErrorResult($"Failed to get total paid: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark a receivable as completed
        /// </summary>
        [HttpPut("{receivableId}/complete")]
        public async Task<ActionResult<ApiResponse<ReceivableDto>>> MarkAsCompleted(string receivableId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReceivableDto>.ErrorResult("User not authenticated"));
                }

                var result = await _receivableService.MarkReceivableAsCompletedAsync(receivableId, userId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ReceivableDto>.ErrorResult($"Failed to mark receivable as completed: {ex.Message}"));
            }
        }
    }
}

