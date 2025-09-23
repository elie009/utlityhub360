using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string userId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only view their own profile unless they're admin
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid();
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult("User not found"));
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return Ok(ApiResponse<UserDto>.SuccessResult(userDto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult($"Failed to get user: {ex.Message}"));
            }
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(string userId, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only update their own profile unless they're admin
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<UserDto>.ErrorResult("Validation failed", errors));
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult("User not found"));
                }

                user.Name = updateUserDto.Name;
                user.Phone = updateUserDto.Phone;
                user.UpdatedAt = DateTime.UtcNow;

                // Only admin can change role and active status
                if (currentUserRole == "ADMIN")
                {
                    user.Role = updateUserDto.Role;
                    user.IsActive = updateUserDto.IsActive;
                }

                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "User updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult($"Failed to update user: {ex.Message}"));
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<UserDto>>>> GetUsers(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.Name)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var userDtos = users.Select(user => new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }).ToList();

                var paginatedResponse = new PaginatedResponse<UserDto>
                {
                    Data = userDtos,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return Ok(ApiResponse<PaginatedResponse<UserDto>>.SuccessResult(paginatedResponse));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<UserDto>>.ErrorResult($"Failed to get users: {ex.Message}"));
            }
        }
    }
}

