using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowAll")]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorsController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<VendorDto>>> CreateVendor([FromBody] CreateVendorDto createVendorDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<VendorDto>.ErrorResult("User not authenticated"));

                var result = await _vendorService.CreateVendorAsync(createVendorDto, userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<VendorDto>.ErrorResult($"Failed to create vendor: {ex.Message}"));
            }
        }

        [HttpGet("{vendorId}")]
        public async Task<ActionResult<ApiResponse<VendorDto>>> GetVendor(string vendorId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<VendorDto>.ErrorResult("User not authenticated"));

                var result = await _vendorService.GetVendorAsync(vendorId, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<VendorDto>.ErrorResult($"Failed to get vendor: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<VendorDto>>>> GetVendors([FromQuery] bool? isActive = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<VendorDto>>.ErrorResult("User not authenticated"));

                var result = await _vendorService.GetVendorsAsync(userId, isActive);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<VendorDto>>.ErrorResult($"Failed to get vendors: {ex.Message}"));
            }
        }

        [HttpPut("{vendorId}")]
        public async Task<ActionResult<ApiResponse<VendorDto>>> UpdateVendor(
            string vendorId,
            [FromBody] UpdateVendorDto updateVendorDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<VendorDto>.ErrorResult("User not authenticated"));

                var result = await _vendorService.UpdateVendorAsync(vendorId, updateVendorDto, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<VendorDto>.ErrorResult($"Failed to update vendor: {ex.Message}"));
            }
        }

        [HttpDelete("{vendorId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteVendor(string vendorId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));

                var result = await _vendorService.DeleteVendorAsync(vendorId, userId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete vendor: {ex.Message}"));
            }
        }
    }
}

