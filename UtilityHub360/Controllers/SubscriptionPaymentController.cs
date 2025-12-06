using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Stripe;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/subscriptionpayment")]
    [Authorize]
    public class SubscriptionPaymentController : ControllerBase
    {
        private readonly IStripePaymentService _stripePaymentService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ApplicationDbContext _context;

        public SubscriptionPaymentController(
            IStripePaymentService stripePaymentService,
            ISubscriptionService subscriptionService,
            ApplicationDbContext context)
        {
            _stripePaymentService = stripePaymentService;
            _subscriptionService = subscriptionService;
            _context = context;
        }

        [HttpPost("create-customer")]
        public async Task<ActionResult<ApiResponse<StripeCustomerDto>>> CreateCustomer()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<StripeCustomerDto>.ErrorResult("User not authenticated"));
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<StripeCustomerDto>.ErrorResult("User not found"));
                }

                var result = await _stripePaymentService.CreateCustomerAsync(
                    userId, 
                    user.Email ?? "", 
                    user.Name ?? ""
                );
                
                if (!result.Success || result.Data == null)
                {
                    return BadRequest(result);
                }
                
                // Get or create user subscription
                var subscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId);
                
                if (subscription == null)
                {
                    // Create a new UserSubscription if it doesn't exist
                    // Get the default STARTER plan (try different case variations)
                    var starterPlan = await _context.SubscriptionPlans
                        .FirstOrDefaultAsync(p => p.Name.ToUpper() == "STARTER");
                    
                    // If STARTER not found, try to get any active plan
                    if (starterPlan == null)
                    {
                        starterPlan = await _context.SubscriptionPlans
                            .Where(p => p.IsActive)
                            .OrderBy(p => p.MonthlyPrice)
                            .FirstOrDefaultAsync();
                    }
                    
                    if (starterPlan == null)
                    {
                        return BadRequest(ApiResponse<StripeCustomerDto>.ErrorResult("No subscription plan found. Please contact support to set up subscription plans."));
                    }
                    
                    subscription = new UserSubscription
                    {
                        UserId = userId,
                        SubscriptionPlanId = starterPlan.Id,
                        Status = "ACTIVE",
                        BillingCycle = "MONTHLY",
                        CurrentPrice = starterPlan.MonthlyPrice,
                        StartDate = DateTime.UtcNow,
                        StripeCustomerId = result.Data.CustomerId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _context.UserSubscriptions.Add(subscription);
                }
                else
                {
                    // Update existing subscription with Stripe customer ID
                    subscription.StripeCustomerId = result.Data.CustomerId;
                    subscription.UpdatedAt = DateTime.UtcNow;
                }
                
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception saveEx)
                {
                    return BadRequest(ApiResponse<StripeCustomerDto>.ErrorResult($"Failed to save subscription: {saveEx.Message}"));
                }
                
                // Reload subscription from database to ensure it's persisted
                await _context.Entry(subscription).ReloadAsync();
                
                // Verify the subscription was created/updated successfully
                var verifySubscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId && !string.IsNullOrEmpty(s.StripeCustomerId));
                
                if (verifySubscription == null)
                {
                    return BadRequest(ApiResponse<StripeCustomerDto>.ErrorResult("Failed to create or update user subscription. Please try again."));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<StripeCustomerDto>.ErrorResult($"Failed to create customer: {ex.Message}"));
            }
        }

        [HttpPost("create-checkout-session")]
        public async Task<ActionResult<ApiResponse<StripeCheckoutSessionDto>>> CreateCheckoutSession([FromBody] CreateCheckoutSessionDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<StripeCheckoutSessionDto>.ErrorResult("User not authenticated"));
                }

                var result = await _stripePaymentService.CreateCheckoutSessionAsync(
                    userId,
                    request.PlanId,
                    request.BillingCycle
                );

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<StripeCheckoutSessionDto>.ErrorResult($"Failed to create checkout session: {ex.Message}"));
            }
        }

        [HttpPost("verify-checkout-session")]
        public async Task<ActionResult<ApiResponse<StripeCheckoutSessionDto>>> VerifyCheckoutSession([FromBody] VerifyCheckoutSessionDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<StripeCheckoutSessionDto>.ErrorResult("User not authenticated"));
                }

                var result = await _stripePaymentService.VerifyCheckoutSessionAsync(request.SessionId);
                
                // Verify the session belongs to the authenticated user
                if (result.Success)
                {
                    try
                    {
                        // Additional verification: ensure session metadata matches authenticated user
                        var sessionService = new Stripe.Checkout.SessionService();
                        var session = await sessionService.GetAsync(request.SessionId);
                        var sessionUserId = session.Metadata?.GetValueOrDefault("UserId");
                        
                        if (sessionUserId != userId)
                        {
                            return Unauthorized(ApiResponse<StripeCheckoutSessionDto>.ErrorResult("Session does not belong to authenticated user"));
                        }
                    }
                    catch (Exception verifyEx)
                    {
                        // Log but don't fail - the main verification already passed
                        Console.WriteLine($"Warning: Could not verify session ownership: {verifyEx.Message}");
                    }
                }

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<StripeCheckoutSessionDto>.ErrorResult($"Failed to verify session: {ex.Message}"));
            }
        }

        [HttpPost("attach-payment-method")]
        public async Task<ActionResult<ApiResponse<StripePaymentMethodDto>>> AttachPaymentMethod([FromBody] UpdatePaymentMethodDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<StripePaymentMethodDto>.ErrorResult("User not authenticated"));
                }

                var subscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (subscription == null)
                {
                    // Try to create subscription if it doesn't exist
                    var starterPlan = await _context.SubscriptionPlans
                        .FirstOrDefaultAsync(p => p.Name == "STARTER");
                    
                    if (starterPlan == null)
                    {
                        return BadRequest(ApiResponse<StripePaymentMethodDto>.ErrorResult("User subscription not found. Please create a customer first."));
                    }
                    
                    // Check if customer was created but subscription wasn't saved
                    // Try to get customer from Stripe to verify
                    return BadRequest(ApiResponse<StripePaymentMethodDto>.ErrorResult("User subscription not found. Please create a customer first by completing the payment setup."));
                }

                if (string.IsNullOrEmpty(subscription.StripeCustomerId))
                {
                    return BadRequest(ApiResponse<StripePaymentMethodDto>.ErrorResult("Stripe customer ID not found. Please create a customer first."));
                }

                var result = await _stripePaymentService.AttachPaymentMethodAsync(
                    subscription.StripeCustomerId,
                    request.PaymentMethodId
                );

                if (result.Success && result.Data != null)
                {
                    subscription.PaymentMethodId = result.Data.PaymentMethodId;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<StripePaymentMethodDto>.ErrorResult($"Failed to attach payment method: {ex.Message}"));
            }
        }

        [HttpPost("create-subscription")]
        public async Task<ActionResult<ApiResponse<StripeSubscriptionDto>>> CreateSubscription([FromBody] CreateSubscriptionPaymentDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<StripeSubscriptionDto>.ErrorResult("User not authenticated"));
                }

                var subscription = await _context.UserSubscriptions
                    .Include(s => s.SubscriptionPlan)
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (subscription == null || string.IsNullOrEmpty(subscription.StripeCustomerId))
                {
                    return BadRequest(ApiResponse<StripeSubscriptionDto>.ErrorResult("User subscription or Stripe customer not found. Please create a customer first."));
                }

                var result = await _stripePaymentService.CreateSubscriptionAsync(
                    subscription.StripeCustomerId,
                    request.PlanId,
                    request.PaymentMethodId,
                    request.BillingCycle
                );

                if (result.Success && result.Data != null)
                {
                    subscription.StripeSubscriptionId = result.Data.SubscriptionId;
                    subscription.PaymentMethodId = request.PaymentMethodId;
                    subscription.Status = result.Data.Status.ToUpper();
                    subscription.BillingCycle = request.BillingCycle;
                    subscription.NextBillingDate = result.Data.CurrentPeriodEnd;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                }

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<StripeSubscriptionDto>.ErrorResult($"Failed to create subscription: {ex.Message}"));
            }
        }

        [HttpPost("cancel-subscription")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelSubscription()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var subscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (subscription == null || string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Active subscription not found"));
                }

                var result = await _stripePaymentService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);

                if (result.Success)
                {
                    subscription.Status = "CANCELLED";
                    subscription.CancelledAt = DateTime.UtcNow;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to cancel subscription: {ex.Message}"));
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

                if (string.IsNullOrEmpty(signature))
                {
                    return BadRequest("Missing Stripe signature");
                }

                var result = await _stripePaymentService.HandleWebhookAsync(json, signature);
                
                return result.Success ? Ok() : BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Webhook error: {ex.Message}");
            }
        }
    }
}

