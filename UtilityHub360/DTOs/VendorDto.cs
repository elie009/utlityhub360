using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class VendorDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Category { get; set; }
        public string? AccountNumber { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int BillCount { get; set; } // Number of bills from this vendor
        public decimal TotalPaid { get; set; } // Total amount paid to this vendor
    }

    public class CreateVendorDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        [Url]
        public string? Website { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? AccountNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateVendorDto
    {
        [StringLength(255)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        [Url]
        public string? Website { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? AccountNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool? IsActive { get; set; }
    }
}

