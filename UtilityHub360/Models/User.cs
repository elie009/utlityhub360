using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? LastModifiedDate { get; set; }
        
        public bool IsActive { get; set; }
    }
}
