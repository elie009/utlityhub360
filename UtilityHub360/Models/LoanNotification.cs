using System;
using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.Models
{
    /// <summary>
    /// Represents a notification sent to a user
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        public NotificationType Type { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReadAt { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
    }
    
    public enum NotificationType
    {
        PAYMENT_DUE,
        PAYMENT_RECEIVED,
        LOAN_APPROVED,
        LOAN_REJECTED,
        GENERAL
    }
}