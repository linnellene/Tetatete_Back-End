using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.User.Services;

public class StripeService : IStripeService
{
    private readonly DataContext _dataContext;
    private readonly CustomerService _customerService;
    private readonly SessionService _sessionService;
    private readonly List<string> _paymentMethods;
    private readonly string _successUrl;
    private readonly string _cancelUrl;

    public StripeService(DataContext dataContext, IConfiguration configuration)
    {
        _dataContext = dataContext;
        _customerService = new CustomerService();
        _sessionService = new SessionService();

        var paymentMethods = configuration.GetSection("Stripe:PaymentMethods").Get<List<string>>();
        var successUrl = configuration.GetSection("Stripe:SuccessUrl").Get<string>();
        var cancelUrl = configuration.GetSection("Stripe:CancelUrl").Get<string>();

        if (paymentMethods is null || successUrl is null || cancelUrl is null)
        {
            throw new ArgumentException("Invalid stripe configuration.");
        }

        _paymentMethods = paymentMethods;
        _successUrl = successUrl;
        _cancelUrl = cancelUrl;
    }

    public async Task<bool> CheckIfSubscriptionIsPaid(Guid userId)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            throw new ArgumentException("Invalid user id.");
        }

        return user.IsPaidSubscription;
    }

    public async Task<string> CreateCheckoutSession(Guid userId, string priceId)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            throw new ArgumentException("Invalid user id.");
        }

        if (user.IsPaidSubscription)
        {
            throw new ArgumentException("Already paid.");
        }

        if (user.CustomerId is null)
        {
            await CreateCustomerForUser(user);
        }

        var sessionOptions = new SessionCreateOptions
        {
            Customer = user.CustomerId,
            PaymentMethodTypes = _paymentMethods,
            Mode = "subscription",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                }
            ],
            SuccessUrl = _successUrl,
            CancelUrl = _cancelUrl,
        };

        var session = await _sessionService.CreateAsync(sessionOptions);

        return session.Url;
    }

    public async Task UpdateSubscriptionStatusForUser(string customerId, bool status, string? subscriptionId, DateTimeOffset? expiresAt)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u =>
            u.CustomerId == customerId);

        if (user is null)
        {
            throw new ArgumentException("Invalid customer id.");
        }

        user.IsPaidSubscription = status;
        user.SubscriptionExpiresAt = expiresAt;
        user.SubscriptionId = subscriptionId;

        await _dataContext.SaveChangesAsync();
    }
    
    public async Task CancelSubscription(Guid userId)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u =>
            u.Id == userId);

        if (user is null)
        {
            throw new ArgumentException("Invalid user id.");
        }
        
        var options = new SubscriptionCancelOptions
        {
            InvoiceNow = false,
            // TODO: clarify
            // Prorate = true 
        };

        var subscriptionService = new SubscriptionService();
        
        await subscriptionService.CancelAsync(user.SubscriptionId, options);

        user.SubscriptionId = null;
        user.SubscriptionExpiresAt = null;

        await _dataContext.SaveChangesAsync();
    }

    private async Task CreateCustomerForUser(UserEntity user)
    {
        if (user.CustomerId is not null)
        {
            throw new ArgumentException("Customer already exists");
        }

        var customerOptions = new CustomerCreateOptions
        {
            Email = user.Email,
        };

        var customer = await _customerService.CreateAsync(customerOptions);

        user.CustomerId = customer.Id;

        await _dataContext.SaveChangesAsync();
    }
}