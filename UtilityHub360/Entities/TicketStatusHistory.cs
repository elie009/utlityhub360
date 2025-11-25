using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class TicketStatusHistory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string TicketId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string OldStatus { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NewStatus { get; set; } = string.Empty;

        [StringLength(450)]
        public string? ChangedBy { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Notes { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; } = null!;

        [ForeignKey("ChangedBy")]
        public virtual User? ChangedByUser { get; set; }
    }
}

