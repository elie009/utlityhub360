namespace UtilityHub360.DTOs
{
    public class SubscriptionPlanDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }
        public int? MaxBankAccounts { get; set; }
        public int? MaxTransactionsPerMonth { get; set; }
        public int? MaxBillsPerMonth { get; set; }
        public int? MaxLoans { get; set; }
        public int? MaxSavingsGoals { get; set; }
        public int? MaxReceiptOcrPerMonth { get; set; }
        public int? MaxAiQueriesPerMonth { get; set; }
        public int? MaxApiCallsPerMonth { get; set; }
        public int? MaxUsers { get; set; }
        public int? TransactionHistoryMonths { get; set; }
        public bool HasAiAssistant { get; set; }
        public bool HasBankFeedIntegration { get; set; }
        public bool HasReceiptOcr { get; set; }
        public bool HasAdvancedReports { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasInvestmentTracking { get; set; }
        public bool HasTaxOptimization { get; set; }
        public bool HasMultiUserSupport { get; set; }
        public bool HasWhiteLabelOptions { get; set; }
        public bool HasCustomIntegrations { get; set; }
        public bool HasDedicatedSupport { get; set; }
        public bool HasAccountManager { get; set; }
        public bool HasCustomReporting { get; set; }
        public bool HasAdvancedSecurity { get; set; }
        public bool HasComplianceReports { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSubscriptionPlanDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }
        public int? MaxBankAccounts { get; set; }
        public int? MaxTransactionsPerMonth { get; set; }
        public int? MaxBillsPerMonth { get; set; }
        public int? MaxLoans { get; set; }
        public int? MaxSavingsGoals { get; set; }
        public int? MaxReceiptOcrPerMonth { get; set; }
        public int? MaxAiQueriesPerMonth { get; set; }
        public int? MaxApiCallsPerMonth { get; set; }
        public int? MaxUsers { get; set; }
        public int? TransactionHistoryMonths { get; set; }
        public bool HasAiAssistant { get; set; }
        public bool HasBankFeedIntegration { get; set; }
        public bool HasReceiptOcr { get; set; }
        public bool HasAdvancedReports { get; set; }
        public bool HasPrioritySupport { get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasInvestmentTracking { get; set; }
        public bool HasTaxOptimization { get; set; }
        public bool HasMultiUserSupport { get; set; }
        public bool HasWhiteLabelOptions { get; set; }
        public bool HasCustomIntegrations { get; set; }
        public bool HasDedicatedSupport { get; set; }
        public bool HasAccountManager { get; set; }
        public bool HasCustomReporting { get; set; }
        public bool HasAdvancedSecurity { get; set; }
        public bool HasComplianceReports { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateSubscriptionPlanDto
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public decimal? YearlyPrice { get; set; }
        public int? MaxBankAccounts { get; set; }
        public int? MaxTransactionsPerMonth { get; set; }
        public int? MaxBillsPerMonth { get; set; }
        public int? MaxLoans { get; set; }
        public int? MaxSavingsGoals { get; set; }
        public int? MaxReceiptOcrPerMonth { get; set; }
        public int? MaxAiQueriesPerMonth { get; set; }
        public int? MaxApiCallsPerMonth { get; set; }
        public int? MaxUsers { get; set; }
        public int? TransactionHistoryMonths { get; set; }
        public bool? HasAiAssistant { get; set; }
        public bool? HasBankFeedIntegration { get; set; }
        public bool? HasReceiptOcr { get; set; }
        public bool? HasAdvancedReports { get; set; }
        public bool? HasPrioritySupport { get; set; }
        public bool? HasApiAccess { get; set; }
        public bool? HasInvestmentTracking { get; set; }
        public bool? HasTaxOptimization { get; set; }
        public bool? HasMultiUserSupport { get; set; }
        public bool? HasWhiteLabelOptions { get; set; }
        public bool? HasCustomIntegrations { get; set; }
        public bool? HasDedicatedSupport { get; set; }
        public bool? HasAccountManager { get; set; }
        public bool? HasCustomReporting { get; set; }
        public bool? HasAdvancedSecurity { get; set; }
        public bool? HasComplianceReports { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class UserSubscriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string PlanDisplayName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public int TransactionsThisMonth { get; set; }
        public int BillsThisMonth { get; set; }
        public int ReceiptOcrThisMonth { get; set; }
        public int AiQueriesThisMonth { get; set; }
        public int ApiCallsThisMonth { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateUserSubscriptionDto
    {
        public string UserId { get; set; } = string.Empty;
        public string SubscriptionPlanId { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = "MONTHLY"; // MONTHLY, YEARLY
        public DateTime? StartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
    }

    public class UpdateUserSubscriptionDto
    {
        public string? SubscriptionPlanId { get; set; }
        public string? Status { get; set; } // ACTIVE, CANCELLED, EXPIRED, SUSPENDED
        public string? BillingCycle { get; set; } // MONTHLY, YEARLY
        public DateTime? EndDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
    }

    public class UserWithSubscriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? SubscriptionPlanId { get; set; }
        public string? SubscriptionPlanName { get; set; }
        public string? SubscriptionStatus { get; set; }
        public string? SubscriptionBillingCycle { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

