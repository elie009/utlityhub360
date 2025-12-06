using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly string _secretKey;
        private readonly string _webhookSecret;

        public StripePaymentService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
            _secretKey = _configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe SecretKey not configured");
            _webhookSecret = _configuration["Stripe:WebhookSecret"] ?? string.Empty;
            StripeConfiguration.ApiKey = _secretKey;
        }

        public async Task<ApiResponse<StripeCustomerDto>> CreateCustomerAsync(string userId, string email, string name)
        {
            try
            {
                var customerService = new CustomerService();
                var customerOptions = new CustomerCreateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = new Dictionary<string, string>
                    {
                        { "UserId", userId }
                    }
                };

                var customer = await customerService.CreateAsync(customerOptions);

                return ApiResponse<StripeCustomerDto>.SuccessResult(new StripeCustomerDto
                {
                    CustomerId = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripeCustomerDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripeCustomerDto>.ErrorResult($"Failed to create customer: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripePaymentMethodDto>> AttachPaymentMethodAsync(string customerId, string paymentMethodId)
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var attachOptions = new PaymentMethodAttachOptions
                {
                    Customer = customerId
                };

                var paymentMethod = await paymentMethodService.AttachAsync(paymentMethodId, attachOptions);

                // Set as default payment method
                var customerService = new CustomerService();
                var customerUpdateOptions = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };
                await customerService.UpdateAsync(customerId, customerUpdateOptions);

                return ApiResponse<StripePaymentMethodDto>.SuccessResult(new StripePaymentMethodDto
                {
                    PaymentMethodId = paymentMethod.Id,
                    Type = paymentMethod.Type,
                    CardLast4 = paymentMethod.Card?.Last4,
                    CardBrand = paymentMethod.Card?.Brand
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripePaymentMethodDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripePaymentMethodDto>.ErrorResult($"Failed to attach payment method: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripeSubscriptionDto>> CreateSubscriptionAsync(string customerId, string planId, string paymentMethodId, string billingCycle)
        {
            try
            {
                // Get the plan from database to retrieve Stripe Price ID
                var plan = await _context.SubscriptionPlans.FindAsync(planId);
                if (plan == null)
                {
                    return ApiResponse<StripeSubscriptionDto>.ErrorResult("Subscription plan not found");
                }

                // Get the Stripe Price ID based on billing cycle
                var priceId = billingCycle.ToUpper() == "YEARLY" 
                    ? plan.StripeYearlyPriceId 
                    : plan.StripeMonthlyPriceId;

                if (string.IsNullOrEmpty(priceId))
                {
                    return ApiResponse<StripeSubscriptionDto>.ErrorResult($"Stripe price ID not configured for {billingCycle} billing cycle");
                }

                var subscriptionService = new Stripe.SubscriptionService();
                var subscriptionOptions = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = priceId
                        }
                    },
                    DefaultPaymentMethod = paymentMethodId,
                    PaymentBehavior = "default_incomplete",
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        SaveDefaultPaymentMethod = "on_subscription"
                    },
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                };

                var subscription = await subscriptionService.CreateAsync(subscriptionOptions);

                return ApiResponse<StripeSubscriptionDto>.SuccessResult(new StripeSubscriptionDto
                {
                    SubscriptionId = subscription.Id,
                    Status = subscription.Status,
                    CurrentPeriodStart = subscription.CurrentPeriodStart,
                    CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                    ClientSecret = subscription.LatestInvoice?.PaymentIntent?.ClientSecret
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripeSubscriptionDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripeSubscriptionDto>.ErrorResult($"Failed to create subscription: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripeSubscriptionDto>> UpdateSubscriptionAsync(string subscriptionId, string? newPlanId = null, string? paymentMethodId = null)
        {
            try
            {
                var subscriptionService = new Stripe.SubscriptionService();
                var updateOptions = new SubscriptionUpdateOptions();

                if (!string.IsNullOrEmpty(paymentMethodId))
                {
                    updateOptions.DefaultPaymentMethod = paymentMethodId;
                }

                if (!string.IsNullOrEmpty(newPlanId))
                {
                    var plan = await _context.SubscriptionPlans.FindAsync(newPlanId);
                    if (plan == null)
                    {
                        return ApiResponse<StripeSubscriptionDto>.ErrorResult("Subscription plan not found");
                    }

                    // Get existing subscription to determine current billing cycle
                    var existingSubscription = await subscriptionService.GetAsync(subscriptionId);
                    var billingInterval = existingSubscription.Items.Data[0].Price.Recurring?.Interval ?? "month";
                    
                    var priceId = billingInterval == "year" 
                        ? plan.StripeYearlyPriceId 
                        : plan.StripeMonthlyPriceId;

                    if (string.IsNullOrEmpty(priceId))
                    {
                        return ApiResponse<StripeSubscriptionDto>.ErrorResult("Stripe price ID not configured for this plan");
                    }

                    updateOptions.Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Id = existingSubscription.Items.Data[0].Id,
                            Price = priceId
                        }
                    };
                    updateOptions.ProrationBehavior = "create_prorations";
                }

                var updatedSubscription = await subscriptionService.UpdateAsync(subscriptionId, updateOptions);

                return ApiResponse<StripeSubscriptionDto>.SuccessResult(new StripeSubscriptionDto
                {
                    SubscriptionId = updatedSubscription.Id,
                    Status = updatedSubscription.Status,
                    CurrentPeriodStart = updatedSubscription.CurrentPeriodStart,
                    CurrentPeriodEnd = updatedSubscription.CurrentPeriodEnd
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripeSubscriptionDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripeSubscriptionDto>.ErrorResult($"Failed to update subscription: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CancelSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var subscriptionService = new Stripe.SubscriptionService();
                var cancelOptions = new SubscriptionCancelOptions
                {
                    InvoiceNow = false,
                    Prorate = false
                };

                await subscriptionService.CancelAsync(subscriptionId, cancelOptions);
                return ApiResponse<bool>.SuccessResult(true, "Subscription cancelled successfully");
            }
            catch (StripeException ex)
            {
                return ApiResponse<bool>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to cancel subscription: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripePaymentIntentDto>> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, string? paymentMethodId = null)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    Customer = customerId,
                    PaymentMethod = paymentMethodId,
                    ConfirmationMethod = "manual",
                    Confirm = false
                };

                var paymentIntent = await paymentIntentService.CreateAsync(options);

                return ApiResponse<StripePaymentIntentDto>.SuccessResult(new StripePaymentIntentDto
                {
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Status = paymentIntent.Status,
                    Amount = amount,
                    Currency = currency
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripePaymentIntentDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripePaymentIntentDto>.ErrorResult($"Failed to create payment intent: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripePaymentMethodDto>> CreatePaymentMethodAsync(string cardToken)
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var options = new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardOptions
                    {
                        Token = cardToken
                    }
                };

                var paymentMethod = await paymentMethodService.CreateAsync(options);

                return ApiResponse<StripePaymentMethodDto>.SuccessResult(new StripePaymentMethodDto
                {
                    PaymentMethodId = paymentMethod.Id,
                    Type = paymentMethod.Type,
                    CardLast4 = paymentMethod.Card?.Last4,
                    CardBrand = paymentMethod.Card?.Brand
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripePaymentMethodDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripePaymentMethodDto>.ErrorResult($"Failed to create payment method: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripeSubscriptionDto>> GetSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var subscriptionService = new Stripe.SubscriptionService();
                var subscription = await subscriptionService.GetAsync(subscriptionId);

                return ApiResponse<StripeSubscriptionDto>.SuccessResult(new StripeSubscriptionDto
                {
                    SubscriptionId = subscription.Id,
                    Status = subscription.Status,
                    CurrentPeriodStart = subscription.CurrentPeriodStart,
                    CurrentPeriodEnd = subscription.CurrentPeriodEnd
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripeSubscriptionDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripeSubscriptionDto>.ErrorResult($"Failed to get subscription: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripeCustomerDto>> GetCustomerAsync(string customerId)
        {
            try
            {
                var customerService = new CustomerService();
                var customer = await customerService.GetAsync(customerId);

                return ApiResponse<StripeCustomerDto>.SuccessResult(new StripeCustomerDto
                {
                    CustomerId = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripeCustomerDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripeCustomerDto>.ErrorResult($"Failed to get customer: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripeCheckoutSessionDto>> CreateCheckoutSessionAsync(string userId, string planId, string billingCycle)
        {
            try
            {
                // Get user and plan from database
                var user = await _context.Users.FindAsync(userId);
                var plan = await _context.SubscriptionPlans.FindAsync(planId);

                if (user == null)
                {
                    return ApiResponse<StripeCheckoutSessionDto>.ErrorResult("User not found");
                }

                if (plan == null)
                {
                    return ApiResponse<StripeCheckoutSessionDto>.ErrorResult("Subscription plan not found");
                }

                // Get or create Stripe customer
                var subscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                string customerId = null;
                if (subscription != null && !string.IsNullOrEmpty(subscription.StripeCustomerId))
                {
                    customerId = subscription.StripeCustomerId;
                }
                else
                {
                    // Create customer if doesn't exist
                    var customerResult = await CreateCustomerAsync(userId, user.Email ?? "", user.Name ?? "");
                    if (customerResult.Success && customerResult.Data != null)
                    {
                        customerId = customerResult.Data.CustomerId;
                    }
                    else
                    {
                        return ApiResponse<StripeCheckoutSessionDto>.ErrorResult("Failed to create Stripe customer");
                    }
                }

                // Get price ID based on billing cycle
                var priceId = billingCycle.ToUpper() == "YEARLY"
                    ? plan.StripeYearlyPriceId
                    : plan.StripeMonthlyPriceId;

                if (string.IsNullOrEmpty(priceId))
                {
                    return ApiResponse<StripeCheckoutSessionDto>.ErrorResult(
                        $"Stripe price ID not configured for plan '{plan.Name}' ({plan.DisplayName}) with {billingCycle} billing cycle. " +
                        $"Please configure StripeMonthlyPriceId or StripeYearlyPriceId in the SubscriptionPlans table.");
                }

                // Create Checkout Session
                var baseUrl = (_configuration["AppSettings:BaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
                var sessionService = new Stripe.Checkout.SessionService();
                var sessionOptions = new Stripe.Checkout.SessionCreateOptions
                {
                    Customer = customerId,
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                    {
                        new Stripe.Checkout.SessionLineItemOptions
                        {
                            Price = priceId,
                            Quantity = 1,
                        },
                    },
                    Mode = "subscription",
                    SuccessUrl = $"{baseUrl}/subscription/success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{baseUrl}/subscription/cancel",
                    ClientReferenceId = userId,
                    Metadata = new Dictionary<string, string>
                    {
                        { "UserId", userId },
                        { "PlanId", planId },
                        { "BillingCycle", billingCycle }
                    },
                    AllowPromotionCodes = true,
                    SubscriptionData = new Stripe.Checkout.SessionSubscriptionDataOptions
                    {
                        Metadata = new Dictionary<string, string>
                        {
                            { "UserId", userId },
                            { "PlanId", planId }
                        }
                    }
                };

                var session = await sessionService.CreateAsync(sessionOptions);

                return ApiResponse<StripeCheckoutSessionDto>.SuccessResult(new StripeCheckoutSessionDto
                {
                    SessionId = session.Id,
                    Url = session.Url
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripeCheckoutSessionDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripeCheckoutSessionDto>.ErrorResult($"Failed to create checkout session: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StripeCheckoutSessionDto>> VerifyCheckoutSessionAsync(string sessionId)
        {
            try
            {
                var sessionService = new Stripe.Checkout.SessionService();
                var session = await sessionService.GetAsync(sessionId);

                // Verify session is completed and paid
                if (session.PaymentStatus != "paid")
                {
                    return ApiResponse<StripeCheckoutSessionDto>.ErrorResult("Payment not completed. Please complete the payment first.");
                }

                // Extract metadata
                var userId = session.Metadata?.GetValueOrDefault("UserId");
                var planId = session.Metadata?.GetValueOrDefault("PlanId");
                var billingCycle = session.Metadata?.GetValueOrDefault("BillingCycle") ?? "MONTHLY";

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(planId))
                {
                    return ApiResponse<StripeCheckoutSessionDto>.ErrorResult("Invalid session metadata. Missing user or plan information.");
                }

                // Get the plan to determine pricing
                var plan = await _context.SubscriptionPlans.FindAsync(planId);
                if (plan == null)
                {
                    return ApiResponse<StripeCheckoutSessionDto>.ErrorResult("Subscription plan not found");
                }

                // Get or create user subscription
                var userSubscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(us => us.UserId == userId);

                if (userSubscription == null)
                {
                    // Create new subscription
                    userSubscription = new UserSubscription
                    {
                        UserId = userId,
                        SubscriptionPlanId = planId,
                        Status = "ACTIVE",
                        BillingCycle = billingCycle,
                        CurrentPrice = billingCycle == "YEARLY" && plan.YearlyPrice.HasValue 
                            ? plan.YearlyPrice.Value 
                            : plan.MonthlyPrice,
                        StartDate = DateTime.UtcNow,
                        StripeCustomerId = session.CustomerId,
                        StripeSubscriptionId = session.SubscriptionId,
                        NextBillingDate = session.Subscription?.CurrentPeriodEnd,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserSubscriptions.Add(userSubscription);
                }
                else
                {
                    // Update existing subscription
                    userSubscription.SubscriptionPlanId = planId;
                    userSubscription.Status = "ACTIVE";
                    userSubscription.BillingCycle = billingCycle;
                    userSubscription.CurrentPrice = billingCycle == "YEARLY" && plan.YearlyPrice.HasValue 
                        ? plan.YearlyPrice.Value 
                        : plan.MonthlyPrice;
                    userSubscription.StripeSubscriptionId = session.SubscriptionId;
                    userSubscription.NextBillingDate = session.Subscription?.CurrentPeriodEnd;
                    userSubscription.UpdatedAt = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(userSubscription.StripeCustomerId))
                    {
                        userSubscription.StripeCustomerId = session.CustomerId;
                    }
                }

                await _context.SaveChangesAsync();

                return ApiResponse<StripeCheckoutSessionDto>.SuccessResult(new StripeCheckoutSessionDto
                {
                    SessionId = session.Id,
                    Url = session.Url
                });
            }
            catch (StripeException ex)
            {
                return ApiResponse<StripeCheckoutSessionDto>.ErrorResult($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<StripeCheckoutSessionDto>.ErrorResult($"Failed to verify session: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> HandleWebhookAsync(string json, string signature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    _webhookSecret
                );

                // Handle different event types
                switch (stripeEvent.Type)
                {
                    case Events.CheckoutSessionCompleted:
                        await HandleCheckoutSessionCompletedAsync(stripeEvent);
                        break;
                    case Events.CustomerSubscriptionCreated:
                    case Events.CustomerSubscriptionUpdated:
                        await HandleSubscriptionUpdatedAsync(stripeEvent);
                        break;
                    case Events.CustomerSubscriptionDeleted:
                        await HandleSubscriptionDeletedAsync(stripeEvent);
                        break;
                    case Events.InvoicePaymentSucceeded:
                        await HandleInvoicePaymentSucceededAsync(stripeEvent);
                        break;
                    case Events.InvoicePaymentFailed:
                        await HandleInvoicePaymentFailedAsync(stripeEvent);
                        break;
                    case Events.PaymentMethodAttached:
                        // Payment method attached - no action needed
                        break;
                }

                return ApiResponse<bool>.SuccessResult(true, "Webhook processed successfully");
            }
            catch (StripeException ex)
            {
                return ApiResponse<bool>.ErrorResult($"Stripe webhook error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to handle webhook: {ex.Message}");
            }
        }

        private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null) return;

            var userSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.StripeSubscriptionId == subscription.Id);

            if (userSubscription != null)
            {
                userSubscription.Status = subscription.Status.ToUpper();
                userSubscription.NextBillingDate = subscription.CurrentPeriodEnd;
                userSubscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Stripe.Subscription;
            if (subscription == null) return;

            var userSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.StripeSubscriptionId == subscription.Id);

            if (userSubscription != null)
            {
                userSubscription.Status = "CANCELLED";
                userSubscription.CancelledAt = DateTime.UtcNow;
                userSubscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleInvoicePaymentSucceededAsync(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null || invoice.SubscriptionId == null) return;

            var userSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.StripeSubscriptionId == invoice.SubscriptionId);

            if (userSubscription != null)
            {
                // Update next billing date
                userSubscription.NextBillingDate = invoice.PeriodEnd;
                userSubscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice == null || invoice.SubscriptionId == null) return;

            var userSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.StripeSubscriptionId == invoice.SubscriptionId);

            if (userSubscription != null)
            {
                // Optionally update status or send notification
                userSubscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session == null || session.PaymentStatus != "paid") return;

            // Extract metadata
            var userId = session.Metadata?.GetValueOrDefault("UserId");
            var planId = session.Metadata?.GetValueOrDefault("PlanId");
            var billingCycle = session.Metadata?.GetValueOrDefault("BillingCycle") ?? "MONTHLY";

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(planId))
            {
                Console.WriteLine($"Warning: Checkout session completed but missing metadata. SessionId: {session.Id}");
                return;
            }

            // Get the plan
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
            {
                Console.WriteLine($"Warning: Plan not found for checkout session. PlanId: {planId}");
                return;
            }

            // Get or create user subscription
            var userSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (userSubscription == null)
            {
                // Create new subscription
                userSubscription = new UserSubscription
                {
                    UserId = userId,
                    SubscriptionPlanId = planId,
                    Status = "ACTIVE",
                    BillingCycle = billingCycle,
                    CurrentPrice = billingCycle == "YEARLY" && plan.YearlyPrice.HasValue 
                        ? plan.YearlyPrice.Value 
                        : plan.MonthlyPrice,
                    StartDate = DateTime.UtcNow,
                    StripeCustomerId = session.CustomerId,
                    StripeSubscriptionId = session.SubscriptionId,
                    NextBillingDate = session.Subscription?.CurrentPeriodEnd,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserSubscriptions.Add(userSubscription);
            }
            else
            {
                // Update existing subscription
                userSubscription.SubscriptionPlanId = planId;
                userSubscription.Status = "ACTIVE";
                userSubscription.BillingCycle = billingCycle;
                userSubscription.CurrentPrice = billingCycle == "YEARLY" && plan.YearlyPrice.HasValue 
                    ? plan.YearlyPrice.Value 
                    : plan.MonthlyPrice;
                userSubscription.StripeSubscriptionId = session.SubscriptionId;
                userSubscription.NextBillingDate = session.Subscription?.CurrentPeriodEnd;
                userSubscription.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(userSubscription.StripeCustomerId))
                {
                    userSubscription.StripeCustomerId = session.CustomerId;
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Checkout session completed and subscription activated. UserId: {userId}, PlanId: {planId}");
        }
    }
}

