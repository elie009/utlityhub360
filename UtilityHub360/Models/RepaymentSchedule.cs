using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a repayment schedule entry for a loan
    /// </summary>
    [Table("LnRepaymentSchedules")]
    public class RepaymentSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrincipalPortion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InterestPortion { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidDate { get; set; }

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; }

        public virtual ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();
        public virtual ICollection<LoanPenalty> Penalties { get; set; } = new List<LoanPenalty>();

        // Computed properties
        [NotMapped]
        public bool IsOverdue => !IsPaid && DueDate < DateTime.UtcNow.Date;

        [NotMapped]
        public int DaysOverdue => IsOverdue ? (DateTime.UtcNow.Date - DueDate).Days : 0;
    }
}
