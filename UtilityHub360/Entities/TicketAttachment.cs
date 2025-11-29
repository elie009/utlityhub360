using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    public class TicketAttachment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string TicketId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FileType { get; set; } // image/png, application/pdf, etc.

        public long? FileSize { get; set; } // Size in bytes

        [StringLength(450)]
        public string? UploadedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; } = null!;

        [ForeignKey("UploadedBy")]
        public virtual User? UploadedByUser { get; set; }
    }
}

