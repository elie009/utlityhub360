using System.ComponentModel.DataAnnotations;

namespace UtilityHub360.DTOs
{
    // Template DTOs
    public class AllocationTemplateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemTemplate { get; set; }
        public bool IsActive { get; set; }
        public List<AllocationTemplateCategoryDto> Categories { get; set; } = new List<AllocationTemplateCategoryDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AllocationTemplateCategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Percentage { get; set; }
        public int DisplayOrder { get; set; }
        public string? Color { get; set; }
    }

    public class CreateAllocationTemplateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public List<CreateAllocationTemplateCategoryDto> Categories { get; set; } = new List<CreateAllocationTemplateCategoryDto>();
    }

    public class CreateAllocationTemplateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }
    }

    // Plan DTOs
    public class AllocationPlanDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<AllocationCategoryDto> Categories { get; set; } = new List<AllocationCategoryDto>();
        public AllocationSummaryDto Summary { get; set; } = new AllocationSummaryDto();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AllocationCategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal Percentage { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? Color { get; set; }
    }

    public class AllocationSummaryDto
    {
        public decimal TotalAllocated { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal SurplusDeficit { get; set; }
        public decimal AllocationPercentage { get; set; } // Total allocated as % of income
    }

    public class CreateAllocationPlanDto
    {
        [Required]
        [StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MonthlyIncome { get; set; }

        [StringLength(450)]
        public string? TemplateId { get; set; } // Optional: base on template

        [Required]
        public List<CreateAllocationCategoryDto> Categories { get; set; } = new List<CreateAllocationCategoryDto>();
    }

    public class CreateAllocationCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal AllocatedAmount { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }
    }

    public class UpdateAllocationPlanDto
    {
        [StringLength(100)]
        public string? PlanName { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MonthlyIncome { get; set; }

        public List<CreateAllocationCategoryDto>? Categories { get; set; }
    }

    // History DTOs
    public class AllocationHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime PeriodDate { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AllocationHistoryQueryDto
    {
        public string? PlanId { get; set; }
        public string? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Months { get; set; } // Last N months
    }

    // Recommendation DTOs
    public class AllocationRecommendationDto
    {
        public string Id { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? SuggestedAmount { get; set; }
        public decimal? SuggestedPercentage { get; set; }
        public string Priority { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsApplied { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
    }

    // Chart/Visualization DTOs
    public class AllocationChartDataDto
    {
        public List<AllocationChartDataPointDto> DataPoints { get; set; } = new List<AllocationChartDataPointDto>();
        public decimal TotalIncome { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal SurplusDeficit { get; set; }
    }

    public class AllocationChartDataPointDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Percentage { get; set; }
        public string? Color { get; set; }
    }

    public class AllocationTrendDto
    {
        public DateTime PeriodDate { get; set; }
        public List<AllocationTrendCategoryDto> Categories { get; set; } = new List<AllocationTrendCategoryDto>();
        public decimal TotalAllocated { get; set; }
        public decimal TotalActual { get; set; }
    }

    public class AllocationTrendCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
    }

    // Formula/Calculation DTOs
    public class AllocationCalculationDto
    {
        public decimal MonthlyIncome { get; set; }
        public List<AllocationCategoryCalculationDto> Categories { get; set; } = new List<AllocationCategoryCalculationDto>();
        public AllocationSummaryDto Summary { get; set; } = new AllocationSummaryDto();
        public string Formula { get; set; } = string.Empty; // Description of calculation method
    }

    public class AllocationCategoryCalculationDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public decimal CalculatedAmount { get; set; } // Income * Percentage / 100
        public string Formula { get; set; } = string.Empty; // e.g., "$5000 * 50% = $2500"
    }
}

