using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Comprehensive audit log for tracking all user activities, system events, and compliance requirements
    /// </summary>
    public class AuditLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UserEmail { get; set; }

        [StringLength(50)]
        [Required]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, VIEW, LOGIN, LOGOUT, EXPORT, etc.

        [StringLength(50)]
        [Required]
        public string EntityType { get; set; } = string.Empty; // LOAN, BILL, TRANSACTION, USER, etc.

        [StringLength(450)]
        public string? EntityId { get; set; }

        [StringLength(200)]
        public string? EntityName { get; set; }

        [StringLength(50)]
        [Required]
        public string LogType { get; set; } = "USER_ACTIVITY"; // USER_ACTIVITY, SYSTEM_EVENT, SECURITY_EVENT, COMPLIANCE_EVENT

        [StringLength(50)]
        public string? Severity { get; set; } = "INFO"; // INFO, WARNING, ERROR, CRITICAL

        [StringLength(500)]
        [Required]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string? OldValues { get; set; } // JSON string of old values

        [Column(TypeName = "nvarchar(max)")]
        public string? NewValues { get; set; } // JSON string of new values

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? RequestMethod { get; set; } // GET, POST, PUT, DELETE

        [StringLength(500)]
        public string? RequestPath { get; set; }

        [StringLength(450)]
        public string? RequestId { get; set; } // Correlation ID for request tracking

        [StringLength(50)]
        public string? ComplianceType { get; set; } // SOX, GDPR, HIPAA, etc.

        [StringLength(100)]
        public string? Category { get; set; } // FINANCIAL, SECURITY, DATA_ACCESS, etc.

        [StringLength(100)]
        public string? SubCategory { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Metadata { get; set; } // Additional JSON metadata

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}

