using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityHub360.Entities
{
    /// <summary>
    /// Represents an investment account or portfolio
    /// </summary>
    public class Investment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string InvestmentType { get; set; } = string.Empty; // STOCK, BOND, MUTUAL_FUND, ETF, CRYPTO, REAL_ESTATE, OTHER

        [StringLength(50)]
        public string? AccountType { get; set; } // BROKERAGE, RETIREMENT_401K, RETIREMENT_IRA, TAXABLE, etc.

        [StringLength(100)]
        public string? BrokerName { get; set; } // Fidelity, Vanguard, Charles Schwab, etc.

        [StringLength(100)]
        public string? AccountNumber { get; set; } // Masked account number

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialInvestment { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentValue { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCostBasis { get; set; } = 0; // Total amount invested

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnrealizedGainLoss { get; set; } // Current value - Cost basis

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RealizedGainLoss { get; set; } // Gains/losses from sold positions

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TotalReturnPercentage { get; set; } // ((Current Value - Cost Basis) / Cost Basis) * 100

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        [StringLength(450)]
        public string? DeletedBy { get; set; }
        
        [StringLength(500)]
        public string? DeleteReason { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<InvestmentPosition> Positions { get; set; } = new List<InvestmentPosition>();
        public virtual ICollection<InvestmentTransaction> Transactions { get; set; } = new List<InvestmentTransaction>();
    }

    /// <summary>
    /// Represents a position (holding) in an investment account
    /// </summary>
    public class InvestmentPosition
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string InvestmentId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Symbol { get; set; } = string.Empty; // Stock ticker, fund symbol, etc.

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty; // Company name, fund name, etc.

        [Required]
        [StringLength(50)]
        public string AssetType { get; set; } = string.Empty; // STOCK, BOND, MUTUAL_FUND, ETF, CRYPTO, REAL_ESTATE, OTHER

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; } // Number of shares/units

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AverageCostBasis { get; set; } // Average price per share/unit

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCostBasis { get; set; } // Quantity * AverageCostBasis

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentPrice { get; set; } // Current market price per share/unit

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentValue { get; set; } // Quantity * CurrentPrice

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnrealizedGainLoss { get; set; } // CurrentValue - TotalCostBasis

        [Column(TypeName = "decimal(5,2)")]
        public decimal? GainLossPercentage { get; set; } // (UnrealizedGainLoss / TotalCostBasis) * 100

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DividendsReceived { get; set; } // Total dividends received for this position

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InterestReceived { get; set; } // Total interest received (for bonds)

        public DateTime? LastPriceUpdate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("InvestmentId")]
        public virtual Investment Investment { get; set; } = null!;
    }

    /// <summary>
    /// Represents a transaction in an investment account (buy, sell, dividend, etc.)
    /// </summary>
    public class InvestmentTransaction
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(450)]
        public string InvestmentId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? PositionId { get; set; } // Associated position if applicable

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } = string.Empty; // BUY, SELL, DIVIDEND, INTEREST, DEPOSIT, WITHDRAWAL, FEE, SPLIT, MERGER

        [Required]
        [StringLength(50)]
        public string Symbol { get; set; } = string.Empty; // Stock ticker, fund symbol, etc.

        [StringLength(200)]
        public string? Name { get; set; } // Company name, fund name, etc.

        [Column(TypeName = "decimal(18,4)")]
        public decimal? Quantity { get; set; } // Number of shares/units (for buy/sell)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PricePerShare { get; set; } // Price per share/unit

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Total transaction amount

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Fees { get; set; } // Transaction fees

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Taxes { get; set; } // Taxes on transaction (if applicable)

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; } // External transaction reference

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete Properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        [StringLength(450)]
        public string? DeletedBy { get; set; }
        
        [StringLength(500)]
        public string? DeleteReason { get; set; }

        // Navigation properties
        [ForeignKey("InvestmentId")]
        public virtual Investment Investment { get; set; } = null!;

        [ForeignKey("PositionId")]
        public virtual InvestmentPosition? Position { get; set; }
    }
}

