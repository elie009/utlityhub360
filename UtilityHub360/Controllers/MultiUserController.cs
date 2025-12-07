using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
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
        private readonly IEmailService _emailService;
        private readonly ILogger<MultiUserController> _logger;

        public MultiUserController(
            ApplicationDbContext context, 
            ISubscriptionService subscriptionService,
            IEmailService emailService,
            ILogger<MultiUserController> logger)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _emailService = emailService;
            _logger = logger;
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

                // Get current user's team
                var team = await _context.Teams
                    .Include(t => t.Members)
                        .ThenInclude(m => m.User)
                    .Include(t => t.Invitations)
                    .FirstOrDefaultAsync(t => t.OwnerId == userId || t.Members.Any(m => m.UserId == userId));

                if (team == null)
                {
                    // User doesn't have a team yet, return empty list
                    return Ok(ApiResponse<List<TeamMemberDto>>.SuccessResult(new List<TeamMemberDto>()));
                }

                // Get all team members
                var teamMembers = await _context.TeamMembers
                    .Include(m => m.User)
                    .Where(m => m.TeamId == team.Id)
                    .ToListAsync();

                // Get pending invitations
                var pendingInvitations = await _context.TeamInvitations
                    .Where(i => i.TeamId == team.Id && i.Status == "PENDING" && i.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                var result = new List<TeamMemberDto>();

                // Add active team members
                foreach (var member in teamMembers.Where(m => m.IsActive))
                {
                    result.Add(new TeamMemberDto
                    {
                        Id = member.Id,
                        Name = member.User.Name,
                        Email = member.User.Email,
                        Role = member.Role,
                        Status = "ACTIVE",
                        JoinedAt = member.JoinedAt
                    });
                }

                // Add pending invitations
                foreach (var invitation in pendingInvitations)
                {
                    result.Add(new TeamMemberDto
                    {
                        Id = invitation.Id,
                        Name = invitation.Email, // Use email as name until user accepts
                        Email = invitation.Email,
                        Role = invitation.Role,
                        Status = "PENDING",
                        InvitedAt = invitation.CreatedAt
                    });
                }

                return Ok(ApiResponse<List<TeamMemberDto>>.SuccessResult(result));
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

                // Get or create team for the user
                var team = await _context.Teams
                    .Include(t => t.Members)
                    .Include(t => t.Invitations)
                    .FirstOrDefaultAsync(t => t.OwnerId == userId);

                if (team == null)
                {
                    // Create a new team for the user
                    var currentUser = await _context.Users.FindAsync(userId);
                    if (currentUser == null)
                    {
                        return NotFound(ApiResponse<bool>.ErrorResult("User not found"));
                    }

                    team = new Team
                    {
                        OwnerId = userId,
                        Name = $"{currentUser.Name}'s Team",
                        Description = "Team created automatically",
                        MaxMembers = 10,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Teams.Add(team);
                    await _context.SaveChangesAsync();

                    // Add owner as team member
                    var ownerMember = new TeamMember
                    {
                        TeamId = team.Id,
                        UserId = userId,
                        Role = "OWNER",
                        IsActive = true,
                        JoinedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.TeamMembers.Add(ownerMember);
                    await _context.SaveChangesAsync();
                }

                // Check if team has reached max members
                var activeMemberCount = team.Members.Count(m => m.IsActive);
                if (activeMemberCount >= team.MaxMembers)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult(
                        $"Team has reached the maximum number of members ({team.MaxMembers})."));
                }

                // Check if user is already a member
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == inviteDto.Email);
                if (existingUser != null)
                {
                    var existingMember = await _context.TeamMembers
                        .FirstOrDefaultAsync(m => m.TeamId == team.Id && m.UserId == existingUser.Id);
                    if (existingMember != null && existingMember.IsActive)
                    {
                        return BadRequest(ApiResponse<bool>.ErrorResult("User is already a team member."));
                    }
                }

                // Check if there's a pending invitation for this email
                var existingInvitation = await _context.TeamInvitations
                    .FirstOrDefaultAsync(i => i.TeamId == team.Id && i.Email == inviteDto.Email && i.Status == "PENDING");
                if (existingInvitation != null && existingInvitation.ExpiresAt > DateTime.UtcNow)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("An invitation has already been sent to this email address."));
                }

                // Create invitation
                var invitation = new TeamInvitation
                {
                    TeamId = team.Id,
                    InvitedByUserId = userId,
                    Email = inviteDto.Email,
                    Role = inviteDto.Role,
                    Token = Guid.NewGuid().ToString(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow
                };
                _context.TeamInvitations.Add(invitation);
                await _context.SaveChangesAsync();

                // Send invitation email
                try
                {
                    var inviteUrl = $"{Request.Scheme}://{Request.Host}/accept-invitation?token={invitation.Token}";
                    var emailSubject = $"Invitation to join {team.Name} on UtilityHub360";
                    var emailBody = $@"
                        <h2>You've been invited to join {team.Name}</h2>
                        <p>You have been invited to join a team on UtilityHub360.</p>
                        <p><strong>Role:</strong> {inviteDto.Role}</p>
                        {(string.IsNullOrEmpty(inviteDto.Message) ? "" : $"<p><strong>Message:</strong> {inviteDto.Message}</p>")}
                        <p>Click the link below to accept the invitation:</p>
                        <p><a href=""{inviteUrl}"">{inviteUrl}</a></p>
                        <p>This invitation will expire in 7 days.</p>
                    ";

                    await _emailService.SendEmailAsync(inviteDto.Email, emailSubject, emailBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send invitation email to {Email}", inviteDto.Email);
                    // Don't fail the request if email fails, invitation is still created
                }

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

                // Get or create team for the user
                var team = await _context.Teams
                    .Include(t => t.Members)
                    .FirstOrDefaultAsync(t => t.OwnerId == userId);

                TeamSettingsDto settings;

                if (team == null)
                {
                    // Return default settings if no team exists
                    settings = new TeamSettingsDto
                    {
                        MaxUsers = 10,
                        CurrentUserCount = 0,
                        AllowInvitations = true
                    };
                }
                else
                {
                    var activeMemberCount = team.Members.Count(m => m.IsActive);
                    settings = new TeamSettingsDto
                    {
                        MaxUsers = team.MaxMembers,
                        CurrentUserCount = activeMemberCount,
                        AllowInvitations = activeMemberCount < team.MaxMembers
                    };
                }

                return Ok(ApiResponse<TeamSettingsDto>.SuccessResult(settings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TeamSettingsDto>.ErrorResult($"Failed to get team settings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Accept a team invitation
        /// </summary>
        [HttpPost("accept-invitation")]
        public async Task<ActionResult<ApiResponse<bool>>> AcceptInvitation([FromQuery] string token)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var currentUser = await _context.Users.FindAsync(userId);
                if (currentUser == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("User not found"));
                }

                // Find invitation by token
                var invitation = await _context.TeamInvitations
                    .Include(i => i.Team)
                    .FirstOrDefaultAsync(i => i.Token == token);

                if (invitation == null)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid invitation token."));
                }

                if (invitation.Status != "PENDING")
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("This invitation has already been processed."));
                }

                if (invitation.ExpiresAt < DateTime.UtcNow)
                {
                    invitation.Status = "EXPIRED";
                    await _context.SaveChangesAsync();
                    return BadRequest(ApiResponse<bool>.ErrorResult("This invitation has expired."));
                }

                // Verify email matches
                if (invitation.Email.ToLower() != currentUser.Email.ToLower())
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("This invitation was sent to a different email address."));
                }

                // Check if user is already a member
                var existingMember = await _context.TeamMembers
                    .FirstOrDefaultAsync(m => m.TeamId == invitation.TeamId && m.UserId == userId);
                if (existingMember != null)
                {
                    if (existingMember.IsActive)
                    {
                        invitation.Status = "ACCEPTED";
                        invitation.AcceptedByUserId = userId;
                        invitation.AcceptedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return BadRequest(ApiResponse<bool>.ErrorResult("You are already a member of this team."));
                    }
                    else
                    {
                        // Reactivate existing member
                        existingMember.IsActive = true;
                        existingMember.Role = invitation.Role;
                        existingMember.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // Create new team member
                    var teamMember = new TeamMember
                    {
                        TeamId = invitation.TeamId,
                        UserId = userId,
                        Role = invitation.Role,
                        IsActive = true,
                        JoinedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.TeamMembers.Add(teamMember);
                }

                // Update invitation status
                invitation.Status = "ACCEPTED";
                invitation.AcceptedByUserId = userId;
                invitation.AcceptedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Invitation accepted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to accept invitation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove a team member
        /// Enterprise feature only
        /// </summary>
        [HttpDelete("team-members/{memberId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveTeamMember(string memberId)
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

                // Get team member
                var teamMember = await _context.TeamMembers
                    .Include(m => m.Team)
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == memberId);

                if (teamMember == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Team member not found."));
                }

                // Check if user is team owner or admin
                var userTeamMember = await _context.TeamMembers
                    .FirstOrDefaultAsync(m => m.TeamId == teamMember.TeamId && m.UserId == userId);
                
                if (teamMember.Team.OwnerId != userId && (userTeamMember == null || (userTeamMember.Role != "OWNER" && userTeamMember.Role != "ADMIN")))
                {
                    return Forbid("You don't have permission to remove team members.");
                }

                // Cannot remove owner
                if (teamMember.Role == "OWNER")
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Cannot remove the team owner."));
                }

                // Cannot remove yourself if you're the owner
                if (teamMember.UserId == userId && teamMember.Team.OwnerId == userId)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Team owner cannot remove themselves."));
                }

                // Deactivate member instead of deleting
                teamMember.IsActive = false;
                teamMember.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Team member removed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to remove team member: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update team member role
        /// Enterprise feature only
        /// </summary>
        [HttpPut("team-members/{memberId}/role")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTeamMemberRole(string memberId, [FromBody] UpdateTeamMemberRoleDto updateDto)
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

                // Get team member
                var teamMember = await _context.TeamMembers
                    .Include(m => m.Team)
                    .FirstOrDefaultAsync(m => m.Id == memberId);

                if (teamMember == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Team member not found."));
                }

                // Check if user is team owner
                if (teamMember.Team.OwnerId != userId)
                {
                    return Forbid("Only the team owner can update member roles.");
                }

                // Cannot change owner role
                if (teamMember.Role == "OWNER")
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Cannot change the owner role."));
                }

                teamMember.Role = updateDto.Role;
                teamMember.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Team member role updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to update team member role: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cancel a pending invitation
        /// Enterprise feature only
        /// </summary>
        [HttpDelete("invitations/{invitationId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelInvitation(string invitationId)
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

                // Get invitation
                var invitation = await _context.TeamInvitations
                    .Include(i => i.Team)
                    .FirstOrDefaultAsync(i => i.Id == invitationId);

                if (invitation == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult("Invitation not found."));
                }

                // Check if user is team owner or the one who sent the invitation
                if (invitation.Team.OwnerId != userId && invitation.InvitedByUserId != userId)
                {
                    return Forbid("You don't have permission to cancel this invitation.");
                }

                if (invitation.Status != "PENDING")
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Only pending invitations can be cancelled."));
                }

                invitation.Status = "REJECTED";
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResult(true, "Invitation cancelled successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to cancel invitation: {ex.Message}"));
            }
        }
    }
}

