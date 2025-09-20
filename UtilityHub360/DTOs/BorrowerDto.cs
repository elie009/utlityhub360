using System;
using System.Collections.Generic;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for Borrower
    /// </summary>
    public class BorrowerDto
    {
        public int BorrowerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string GovernmentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
