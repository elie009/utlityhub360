using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class TeamInvitation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string TeamId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string InvitedByUserId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "MEMBER"; // ADMIN, MEMBER, VIEWER

        [Required]
        [StringLength(100)]
        public string Token { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, ACCEPTED, REJECTED, EXPIRED

        [StringLength(450)]
        public string? AcceptedByUserId { get; set; }

        public DateTime? AcceptedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; } = null!;

        [ForeignKey("InvitedByUserId")]
        public virtual User InvitedBy { get; set; } = null!;

        [ForeignKey("AcceptedByUserId")]
        public virtual User? AcceptedBy { get; set; }
    }
}

