using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class TeamMemberDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // ACTIVE, PENDING, INACTIVE
        public DateTime? InvitedAt { get; set; }
        public DateTime? JoinedAt { get; set; }
    }

    public class InviteTeamMemberDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "MEMBER"; // ADMIN, MEMBER, VIEWER

        [StringLength(500)]
        public string? Message { get; set; }
    }

    public class TeamSettingsDto
    {
        public int MaxUsers { get; set; }
        public int CurrentUserCount { get; set; }
        public bool AllowInvitations { get; set; } = true;
    }
}

