using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for managing transaction rules (auto-categorize, auto-tag, auto-approve)
    /// </summary>
    public interface ITransactionRulesService
    {
        /// <summary>
        /// Create a new transaction rule
        /// </summary>
        Task<ApiResponse<TransactionRuleDto>> CreateRuleAsync(CreateTransactionRuleDto rule, string userId);

        /// <summary>
        /// Update an existing transaction rule
        /// </summary>
        Task<ApiResponse<TransactionRuleDto>> UpdateRuleAsync(UpdateTransactionRuleDto rule, string userId);

        /// <summary>
        /// Delete a transaction rule
        /// </summary>
        Task<ApiResponse<bool>> DeleteRuleAsync(string ruleId, string userId);

        /// <summary>
        /// Get all rules for a user
        /// </summary>
        Task<ApiResponse<List<TransactionRuleDto>>> GetRulesAsync(string userId);

        /// <summary>
        /// Apply rules to a transaction
        /// </summary>
        Task<TransactionRuleResult> ApplyRulesAsync(CreateTransactionRequest transaction, string userId);

        /// <summary>
        /// Test a rule against a transaction
        /// </summary>
        Task<bool> TestRuleAsync(TransactionRuleDto rule, CreateTransactionRequest transaction);
    }

    /// <summary>
    /// Transaction rule DTO
    /// </summary>
    public class TransactionRuleDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0; // Higher priority rules are applied first

        // Rule conditions
        public RuleConditionDto Condition { get; set; } = new();

        // Rule actions
        public string? AutoCategory { get; set; }
        public List<string> AutoTags { get; set; } = new();
        public bool AutoApprove { get; set; } = false;
        public string? AutoDescription { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Rule condition DTO
    /// </summary>
    public class RuleConditionDto
    {
        public string Field { get; set; } = string.Empty; // description, amount, merchant, etc.
        public string Operator { get; set; } = string.Empty; // contains, equals, greater_than, less_than, etc.
        public string Value { get; set; } = string.Empty;
        public bool CaseSensitive { get; set; } = false;
    }

    /// <summary>
    /// Create transaction rule DTO
    /// </summary>
    public class CreateTransactionRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0;
        public RuleConditionDto Condition { get; set; } = new();
        public string? AutoCategory { get; set; }
        public List<string> AutoTags { get; set; } = new();
        public bool AutoApprove { get; set; } = false;
        public string? AutoDescription { get; set; }
    }

    /// <summary>
    /// Update transaction rule DTO
    /// </summary>
    public class UpdateTransactionRuleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0;
        public RuleConditionDto Condition { get; set; } = new();
        public string? AutoCategory { get; set; }
        public List<string> AutoTags { get; set; } = new();
        public bool AutoApprove { get; set; } = false;
        public string? AutoDescription { get; set; }
    }

    /// <summary>
    /// Result of applying rules to a transaction
    /// </summary>
    public class TransactionRuleResult
    {
        public bool Matched { get; set; }
        public string? Category { get; set; }
        public List<string> Tags { get; set; } = new();
        public bool AutoApprove { get; set; }
        public string? Description { get; set; }
        public string? MatchedRuleId { get; set; }
        public string? MatchedRuleName { get; set; }
    }
}

