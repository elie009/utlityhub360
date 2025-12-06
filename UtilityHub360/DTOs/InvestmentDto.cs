using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class InvestmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string InvestmentType { get; set; } = string.Empty;
        public string? AccountType { get; set; }
        public string? BrokerName { get; set; }
        public string? AccountNumber { get; set; }
        public decimal InitialInvestment { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalCostBasis { get; set; }
        public decimal? UnrealizedGainLoss { get; set; }
        public decimal? RealizedGainLoss { get; set; }
        public decimal? TotalReturnPercentage { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateInvestmentDto
    {
        [Required]
        [StringLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string InvestmentType { get; set; } = string.Empty;

        [StringLength(50)]
        public string? AccountType { get; set; }

        [StringLength(100)]
        public string? BrokerName { get; set; }

        [StringLength(100)]
        public string? AccountNumber { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal InitialInvestment { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CurrentValue { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateInvestmentDto
    {
        [StringLength(100)]
        public string? AccountName { get; set; }

        [StringLength(50)]
        public string? InvestmentType { get; set; }

        [StringLength(50)]
        public string? AccountType { get; set; }

        [StringLength(100)]
        public string? BrokerName { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CurrentValue { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}

