using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for managing and applying transaction rules
    /// </summary>
    public class TransactionRulesService : ITransactionRulesService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionRulesService> _logger;

        public TransactionRulesService(ApplicationDbContext context, ILogger<TransactionRulesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<TransactionRuleDto>> CreateRuleAsync(CreateTransactionRuleDto rule, string userId)
        {
            try
            {
                var ruleEntity = new TransactionRule
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Name = rule.Name,
                    Description = rule.Description,
                    IsActive = rule.IsActive,
                    Priority = rule.Priority,
                    ConditionField = rule.Condition.Field,
                    ConditionOperator = rule.Condition.Operator,
                    ConditionValue = rule.Condition.Value,
                    ConditionCaseSensitive = rule.Condition.CaseSensitive,
                    AutoCategory = rule.AutoCategory,
                    AutoTags = string.Join(",", rule.AutoTags),
                    AutoApprove = rule.AutoApprove,
                    AutoDescription = rule.AutoDescription,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TransactionRules.Add(ruleEntity);
                await _context.SaveChangesAsync();

                var dto = MapToDto(ruleEntity);
                return ApiResponse<TransactionRuleDto>.SuccessResult(dto, "Rule created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction rule");
                return ApiResponse<TransactionRuleDto>.ErrorResult($"Failed to create rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TransactionRuleDto>> UpdateRuleAsync(UpdateTransactionRuleDto rule, string userId)
        {
            try
            {
                var ruleEntity = await _context.TransactionRules
                    .FirstOrDefaultAsync(r => r.Id == rule.Id && r.UserId == userId);

                if (ruleEntity == null)
                {
                    return ApiResponse<TransactionRuleDto>.ErrorResult("Rule not found");
                }

                ruleEntity.Name = rule.Name;
                ruleEntity.Description = rule.Description;
                ruleEntity.IsActive = rule.IsActive;
                ruleEntity.Priority = rule.Priority;
                ruleEntity.ConditionField = rule.Condition.Field;
                ruleEntity.ConditionOperator = rule.Condition.Operator;
                ruleEntity.ConditionValue = rule.Condition.Value;
                ruleEntity.ConditionCaseSensitive = rule.Condition.CaseSensitive;
                ruleEntity.AutoCategory = rule.AutoCategory;
                ruleEntity.AutoTags = string.Join(",", rule.AutoTags);
                ruleEntity.AutoApprove = rule.AutoApprove;
                ruleEntity.AutoDescription = rule.AutoDescription;
                ruleEntity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var dto = MapToDto(ruleEntity);
                return ApiResponse<TransactionRuleDto>.SuccessResult(dto, "Rule updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction rule {RuleId}", rule.Id);
                return ApiResponse<TransactionRuleDto>.ErrorResult($"Failed to update rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteRuleAsync(string ruleId, string userId)
        {
            try
            {
                var rule = await _context.TransactionRules
                    .FirstOrDefaultAsync(r => r.Id == ruleId && r.UserId == userId);

                if (rule == null)
                {
                    return ApiResponse<bool>.ErrorResult("Rule not found");
                }

                _context.TransactionRules.Remove(rule);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Rule deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction rule {RuleId}", ruleId);
                return ApiResponse<bool>.ErrorResult($"Failed to delete rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TransactionRuleDto>>> GetRulesAsync(string userId)
        {
            try
            {
                var rules = await _context.TransactionRules
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.Priority)
                    .ThenBy(r => r.Name)
                    .ToListAsync();

                var dtos = rules.Select(MapToDto).ToList();
                return ApiResponse<List<TransactionRuleDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction rules for user {UserId}", userId);
                return ApiResponse<List<TransactionRuleDto>>.ErrorResult($"Failed to get rules: {ex.Message}");
            }
        }

        public async Task<TransactionRuleResult> ApplyRulesAsync(CreateTransactionRequest transaction, string userId)
        {
            var result = new TransactionRuleResult();

            try
            {
                var rules = await _context.TransactionRules
                    .Where(r => r.UserId == userId && r.IsActive)
                    .OrderByDescending(r => r.Priority)
                    .ToListAsync();

                foreach (var rule in rules)
                {
                    if (await TestRuleAsync(MapToDto(rule), transaction))
                    {
                        result.Matched = true;
                        result.MatchedRuleId = rule.Id;
                        result.MatchedRuleName = rule.Name;

                        if (!string.IsNullOrEmpty(rule.AutoCategory))
                        {
                            result.Category = rule.AutoCategory;
                        }

                        if (!string.IsNullOrEmpty(rule.AutoTags))
                        {
                            result.Tags = rule.AutoTags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                        }

                        result.AutoApprove = rule.AutoApprove;

                        if (!string.IsNullOrEmpty(rule.AutoDescription))
                        {
                            result.Description = rule.AutoDescription;
                        }

                        // Apply first matching rule (highest priority)
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying transaction rules");
            }

            return result;
        }

        public async Task<bool> TestRuleAsync(TransactionRuleDto rule, CreateTransactionRequest transaction)
        {
            var condition = rule.Condition;
            string? fieldValue = null;

            // Get field value from transaction
            switch (condition.Field.ToLower())
            {
                case "description":
                    fieldValue = transaction.Description;
                    break;
                case "merchant":
                    fieldValue = transaction.MerchantName;
                    break;
                case "amount":
                    fieldValue = transaction.Amount.ToString();
                    break;
                case "category":
                    fieldValue = transaction.Category;
                    break;
                case "type":
                    fieldValue = transaction.TransactionType;
                    break;
            }

            if (fieldValue == null)
            {
                return false;
            }

            // Apply operator
            var comparisonValue = condition.CaseSensitive ? fieldValue : fieldValue.ToLower();
            var ruleValue = condition.CaseSensitive ? condition.Value : condition.Value.ToLower();

            return condition.Operator.ToLower() switch
            {
                "contains" => comparisonValue.Contains(ruleValue),
                "equals" => comparisonValue == ruleValue,
                "starts_with" => comparisonValue.StartsWith(ruleValue),
                "ends_with" => comparisonValue.EndsWith(ruleValue),
                "greater_than" => decimal.TryParse(fieldValue, out var amount) && decimal.TryParse(condition.Value, out var threshold) && amount > threshold,
                "less_than" => decimal.TryParse(fieldValue, out var amount2) && decimal.TryParse(condition.Value, out var threshold2) && amount2 < threshold2,
                "greater_than_or_equal" => decimal.TryParse(fieldValue, out var amount3) && decimal.TryParse(condition.Value, out var threshold3) && amount3 >= threshold3,
                "less_than_or_equal" => decimal.TryParse(fieldValue, out var amount4) && decimal.TryParse(condition.Value, out var threshold4) && amount4 <= threshold4,
                _ => false
            };
        }

        private TransactionRuleDto MapToDto(TransactionRule rule)
        {
            return new TransactionRuleDto
            {
                Id = rule.Id,
                UserId = rule.UserId,
                Name = rule.Name,
                Description = rule.Description,
                IsActive = rule.IsActive,
                Priority = rule.Priority,
                Condition = new RuleConditionDto
                {
                    Field = rule.ConditionField,
                    Operator = rule.ConditionOperator,
                    Value = rule.ConditionValue,
                    CaseSensitive = rule.ConditionCaseSensitive
                },
                AutoCategory = rule.AutoCategory,
                AutoTags = string.IsNullOrEmpty(rule.AutoTags) ? new List<string>() : rule.AutoTags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                AutoApprove = rule.AutoApprove,
                AutoDescription = rule.AutoDescription,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt
            };
        }
    }
}

