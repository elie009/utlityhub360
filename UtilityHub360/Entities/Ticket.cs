using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class Ticket
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "OPEN"; // OPEN, IN_PROGRESS, RESOLVED, CLOSED

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "NORMAL"; // LOW, NORMAL, HIGH, URGENT

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "GENERAL"; // BUG, FEATURE_REQUEST, SUPPORT, TECHNICAL, BILLING, GENERAL

        [StringLength(450)]
        public string? AssignedTo { get; set; } // Admin/Support agent user ID

        [Column(TypeName = "nvarchar(max)")]
        public string? ResolutionNotes { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [StringLength(450)]
        public string? ResolvedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("AssignedTo")]
        public virtual User? AssignedUser { get; set; }

        [ForeignKey("ResolvedBy")]
        public virtual User? ResolvedByUser { get; set; }

        // Collections
        public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
        public virtual ICollection<TicketStatusHistory> StatusHistory { get; set; } = new List<TicketStatusHistory>();
    }
}

