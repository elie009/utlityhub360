namespace UtilityHub360.DTOs
{
    /// <summary>
    /// DTO for Recent Activity section on dashboard
    /// Contains profile completion status and financial summary
    /// </summary>
    public class RecentActivityDto
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Number of income sources configured
        /// </summary>
        public int IncomeSourcesCount { get; set; }
        
        /// <summary>
        /// Total monthly income from all income sources
        /// </summary>
        public decimal TotalMonthlyIncome { get; set; }
        
        /// <summary>
        /// Total monthly goals (savings + investment + emergency fund)
        /// </summary>
        public decimal TotalMonthlyGoals { get; set; }
        
        /// <summary>
        /// Disposable income amount (income - expenses)
        /// </summary>
        public decimal DisposableAmount { get; set; }
        
        /// <summary>
        /// Whether user profile is completed
        /// </summary>
        public bool HasProfile { get; set; }
        
        /// <summary>
        /// Profile completion message
        /// </summary>
        public string ProfileStatus { get; set; } = string.Empty;
    }
}

