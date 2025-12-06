using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IStripePaymentService
    {
        Task<ApiResponse<StripeCustomerDto>> CreateCustomerAsync(string userId, string email, string name);
        Task<ApiResponse<StripePaymentMethodDto>> AttachPaymentMethodAsync(string customerId, string paymentMethodId);
        Task<ApiResponse<StripeSubscriptionDto>> CreateSubscriptionAsync(string customerId, string planId, string paymentMethodId, string billingCycle);
        Task<ApiResponse<StripeSubscriptionDto>> UpdateSubscriptionAsync(string subscriptionId, string? newPlanId = null, string? paymentMethodId = null);
        Task<ApiResponse<bool>> CancelSubscriptionAsync(string subscriptionId);
        Task<ApiResponse<StripePaymentIntentDto>> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, string? paymentMethodId = null);
        Task<ApiResponse<StripePaymentMethodDto>> CreatePaymentMethodAsync(string cardToken);
        Task<ApiResponse<StripeSubscriptionDto>> GetSubscriptionAsync(string subscriptionId);
        Task<ApiResponse<StripeCustomerDto>> GetCustomerAsync(string customerId);
        Task<ApiResponse<StripeCheckoutSessionDto>> CreateCheckoutSessionAsync(string userId, string planId, string billingCycle);
        Task<ApiResponse<StripeCheckoutSessionDto>> VerifyCheckoutSessionAsync(string sessionId);
        Task<ApiResponse<bool>> HandleWebhookAsync(string json, string signature);
    }
}

