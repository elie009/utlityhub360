using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class BillDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string BillName { get; set; } = string.Empty;
        public string BillType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Notes { get; set; }
        public string? Provider { get; set; }
        public string? ReferenceNumber { get; set; }
    }

    public class CreateBillDto
    {
        [Required]
        [StringLength(255)]
        public string BillName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BillType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Frequency { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Provider { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }
    }

    public class UpdateBillDto
    {
        [StringLength(255)]
        public string? BillName { get; set; }

        [StringLength(50)]
        public string? BillType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(20)]
        public string? Frequency { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Provider { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }
    }

    public class BillAnalyticsDto
    {
        public decimal TotalPendingAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalOverdueAmount { get; set; }
        public int TotalPendingBills { get; set; }
        public int TotalPaidBills { get; set; }
        public int TotalOverdueBills { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class BillSummaryDto
    {
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public string Period { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
