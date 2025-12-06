namespace UtilityHub360.DTOs
{
    public class StripeCustomerDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Name { get; set; }
    }

    public class StripePaymentMethodDto
    {
        public string PaymentMethodId { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
    }

    public class StripeSubscriptionDto
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public string? ClientSecret { get; set; }
    }

    public class StripePaymentIntentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string? ClientSecret { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }

    public class CreateSubscriptionPaymentDto
    {
        public string PlanId { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = "MONTHLY"; // MONTHLY or YEARLY
        public string PaymentMethodId { get; set; } = string.Empty;
    }

    public class UpdatePaymentMethodDto
    {
        public string PaymentMethodId { get; set; } = string.Empty;
    }

    public class StripeCheckoutSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class CreateCheckoutSessionDto
    {
        public string PlanId { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = "MONTHLY"; // MONTHLY or YEARLY
    }

    public class VerifyCheckoutSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
    }
}

