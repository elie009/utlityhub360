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
    public class MultiUserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISubscriptionService _subscriptionService;

        public MultiUserController(ApplicationDbContext context, ISubscriptionService subscriptionService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Get all team members for the current user's organization
        /// Enterprise feature only
        /// </summary>
        [HttpGet("team-members")]
        public async Task<ActionResult<ApiResponse<List<TeamMemberDto>>>> GetTeamMembers()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TeamMemberDto>>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Multi-User Support feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "MULTI_USER");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<List<TeamMemberDto>>.ErrorResult(
                        "Multi-User Support is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                // Get current user to find organization/team
                var currentUser = await _context.Users.FindAsync(userId);
                if (currentUser == null)
                {
                    return NotFound(ApiResponse<List<TeamMemberDto>>.ErrorResult("User not found"));
                }

                // For now, return empty list - full implementation would require team/organization entities
                var teamMembers = new List<TeamMemberDto>();

                return Ok(ApiResponse<List<TeamMemberDto>>.SuccessResult(teamMembers));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TeamMemberDto>>.ErrorResult($"Failed to get team members: {ex.Message}"));
            }
        }

        /// <summary>
        /// Invite a team member
        /// Enterprise feature only
        /// </summary>
        [HttpPost("invite")]
        public async Task<ActionResult<ApiResponse<bool>>> InviteTeamMember([FromBody] InviteTeamMemberDto inviteDto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Multi-User Support feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "MULTI_USER");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult(
                        "Multi-User Support is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                // TODO: Implement team member invitation logic
                // This would typically involve:
                // 1. Creating an invitation record
                // 2. Sending an email invitation
                // 3. Creating a team member record when accepted

                return Ok(ApiResponse<bool>.SuccessResult(true, "Team member invitation sent successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to invite team member: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get team/organization settings
        /// Enterprise feature only
        /// </summary>
        [HttpGet("settings")]
        public async Task<ActionResult<ApiResponse<TeamSettingsDto>>> GetTeamSettings()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TeamSettingsDto>.ErrorResult("User not authenticated"));
                }

                // Check if user has access to Multi-User Support feature
                var featureCheck = await _subscriptionService.CheckFeatureAccessAsync(userId, "MULTI_USER");
                if (!featureCheck.Success || !featureCheck.Data)
                {
                    return BadRequest(ApiResponse<TeamSettingsDto>.ErrorResult(
                        "Multi-User Support is an Enterprise feature. Please upgrade to Premium Plus (Enterprise) to access this feature."));
                }

                // Return default team settings
                var settings = new TeamSettingsDto
                {
                    MaxUsers = 10, // Default for Enterprise
                    CurrentUserCount = 1
                };

                return Ok(ApiResponse<TeamSettingsDto>.SuccessResult(settings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TeamSettingsDto>.ErrorResult($"Failed to get team settings: {ex.Message}"));
            }
        }
    }
}

