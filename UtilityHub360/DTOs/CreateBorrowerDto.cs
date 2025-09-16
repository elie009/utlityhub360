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
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(50)]
        public string GovernmentId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }
}
