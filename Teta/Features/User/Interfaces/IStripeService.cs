namespace TetaBackend.Features.User.Interfaces;

public interface IStripeService
{
    Task<bool> CheckIfSubscriptionIsPaid(Guid userId);

    Task<string> CreateCheckoutSession(Guid userId, string priceId);

    Task UpdateSubscriptionStatusForUser(string customerId, bool status, string? subscriptionId,
        DateTimeOffset? expiresAt);

    Task CancelSubscription(Guid userId);
}