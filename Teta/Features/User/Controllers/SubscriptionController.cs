using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Swashbuckle.AspNetCore.Annotations;
using TetaBackend.Features.User.Dto;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.User.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly string _webhookSecret;

    public SubscriptionController(IStripeService stripeService, IConfiguration configuration)
    {
        _stripeService = stripeService;
        var secret = configuration.GetSection("Stripe:WebHookSecret").Value;

        _webhookSecret = secret ?? throw new ArgumentException("Invalid web hook secret");
    }

    [SwaggerOperation(Summary = "Checks if user has paid a subscription.")]
    [HttpGet]
    [Authorize]
    public async Task<ActionResult> CheckIfSubscriptionIsPaid()
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString()!;

            return Ok(new CheckIfSubscriptionIsPaidDto
            {
                IsPaid = await _stripeService.CheckIfSubscriptionIsPaid(new Guid(userId))
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Creates checkout session for Stripe.")]
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionDto dto)
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString()!;

            return Ok(await _stripeService.CreateCheckoutSession(new Guid(userId), dto.PriceId));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Cancels user Stripe subscription.")]
    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> CancelSubscription()
    {
        try
        {
            var userId = HttpContext.Items["UserId"]?.ToString()!;

            await _stripeService.CancelSubscription(new Guid(userId));

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Webhook for Stripe events.")]
    [HttpPost("webHook")]
    public async Task<ActionResult> HandleWebhookEvent()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                _webhookSecret
            );

            switch (stripeEvent.Type)
            {
                // Session success payment
                case "checkout.session.completed":

                    if (stripeEvent.Data.Object is not Session session)
                    {
                        break;
                    }

                    if (session.Mode == "subscription")
                    {
                        await _stripeService.UpdateSubscriptionStatusForUser(session.CustomerId, true,
                            session.Subscription.Id,
                            session.Subscription.CurrentPeriodEnd);
                    }

                    break;

                // Possible subscription renewal if pasting due then repaying
                case "invoice.payment_succeeded":
                    if (stripeEvent.Data.Object is not Invoice invoice)
                    {
                        break;
                    }

                    await _stripeService.UpdateSubscriptionStatusForUser(invoice.CustomerId, true,
                        invoice.Subscription.Id,
                        invoice.Subscription.CurrentPeriodEnd);

                    break;

                // Session error payment
                case "invoice.payment_failed":
                    if (stripeEvent.Data.Object is not Invoice failedInvoice)
                    {
                        break;
                    }

                    await _stripeService.UpdateSubscriptionStatusForUser(failedInvoice.CustomerId, false,
                        null, null);

                    break;

                // Subscription past due and not refilling
                case "customer.subscription.updated":
                    if (stripeEvent.Data.Object is not Subscription subscription)
                    {
                        break;
                    }

                    if (subscription.Status == "past_due")
                    {
                        await _stripeService.UpdateSubscriptionStatusForUser(subscription.CustomerId, false,
                            null,
                            null);
                    }
                    else if (subscription.Status == "active")
                    {
                        await _stripeService.UpdateSubscriptionStatusForUser(subscription.CustomerId, true,
                            subscription.Id,
                            subscription.CurrentPeriodEnd);
                    }

                    break;

                case "customer.subscription.deleted":
                    if (stripeEvent.Data.Object is not Subscription canceledSubscription)
                    {
                        break;
                    }

                    await _stripeService.UpdateSubscriptionStatusForUser(canceledSubscription.CustomerId, false, null,
                        null);
                    break;

                default:
                    return Ok();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}