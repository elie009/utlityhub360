using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    public class HideTransactionDto
    {
        [StringLength(500)]
        public string? Reason { get; set; }
    }
}

