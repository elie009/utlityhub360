using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class CardDto
    {
        public string Id { get; set; } = string.Empty;
        public string BankAccountId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public string? CardBrand { get; set; }
        public string? Last4Digits { get; set; }
        public string? CardholderName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? AccountName { get; set; } // From BankAccount
    }

    public class CreateCardDto
    {
        [Required]
        public string BankAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Card name cannot exceed 100 characters")]
        public string CardName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CardType { get; set; } = string.Empty; // DEBIT, CREDIT, ATM, PREPAID

        [StringLength(50)]
        public string? CardBrand { get; set; } // VISA, MASTERCARD, AMEX

        [StringLength(20)]
        public string? Last4Digits { get; set; } // Last 4 digits only

        [StringLength(100)]
        public string? CardholderName { get; set; }

        [StringLength(10)]
        public string? ExpiryMonth { get; set; } // MM

        [StringLength(10)]
        public string? ExpiryYear { get; set; } // YYYY

        public bool IsPrimary { get; set; } = false;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class UpdateCardDto
    {
        [StringLength(100, ErrorMessage = "Card name cannot exceed 100 characters")]
        public string? CardName { get; set; }

        [StringLength(50)]
        public string? CardType { get; set; }

        [StringLength(50)]
        public string? CardBrand { get; set; }

        [StringLength(20)]
        public string? Last4Digits { get; set; }

        [StringLength(100)]
        public string? CardholderName { get; set; }

        [StringLength(10)]
        public string? ExpiryMonth { get; set; }

        [StringLength(10)]
        public string? ExpiryYear { get; set; }

        public bool? IsPrimary { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }
}

