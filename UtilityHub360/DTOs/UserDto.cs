using System;

namespace UtilityHub360.DTOs
{
    /// <summary>
    /// Data Transfer Object for User entity
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
