using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new borrower
    /// </summary>
    public class CreateBorrowerDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [StringLength(50)]
        public string GovernmentId { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public CreateBorrowerDto()
        {
            Status = "Active";
        }
    }
}
