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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TransactionCategoryDto>>> CreateCategory([FromBody] CreateTransactionCategoryDto createDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TransactionCategoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _categoryService.CreateCategoryAsync(createDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TransactionCategoryDto>.ErrorResult($"Failed to create category: {ex.Message}"));
            }
        }

        [HttpPut("{categoryId}")]
        [HttpPost("{categoryId}/update")]  // POST alternative for environments where PUT is blocked
        public async Task<ActionResult<ApiResponse<TransactionCategoryDto>>> UpdateCategory(string categoryId, [FromBody] UpdateTransactionCategoryDto updateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TransactionCategoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDto, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TransactionCategoryDto>.ErrorResult($"Failed to update category: {ex.Message}"));
            }
        }

        [HttpDelete("{categoryId}")]
        [HttpPost("{categoryId}/delete")]  // POST alternative for environments where DELETE is blocked
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(string categoryId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _categoryService.DeleteCategoryAsync(categoryId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete category: {ex.Message}"));
            }
        }

        [HttpGet("{categoryId}")]
        public async Task<ActionResult<ApiResponse<TransactionCategoryDto>>> GetCategory(string categoryId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TransactionCategoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _categoryService.GetCategoryByIdAsync(categoryId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TransactionCategoryDto>.ErrorResult($"Failed to get category: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TransactionCategoryDto>>>> GetAllCategories([FromQuery] string? type = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TransactionCategoryDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _categoryService.GetAllCategoriesAsync(userId, type);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionCategoryDto>>.ErrorResult($"Failed to get categories: {ex.Message}"));
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<TransactionCategoryDto>>>> GetActiveCategories([FromQuery] string? type = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TransactionCategoryDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _categoryService.GetActiveCategoriesAsync(userId, type);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionCategoryDto>>.ErrorResult($"Failed to get active categories: {ex.Message}"));
            }
        }

        [HttpPost("seed-system")]
        public async Task<ActionResult<ApiResponse<bool>>> SeedSystemCategories()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _categoryService.SeedSystemCategoriesAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to seed system categories: {ex.Message}"));
            }
        }
    }
}

