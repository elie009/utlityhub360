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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string GovernmentId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FullName { get; set; }
    }
}