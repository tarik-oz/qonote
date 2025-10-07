using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Subscriptions._Shared;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Subscriptions.CreateCheckout;

public sealed class CreateCheckoutCommandHandler : IRequestHandler<CreateCheckoutCommand, CheckoutUrlDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;
    private readonly IPaymentService _paymentService;

    public CreateCheckoutCommandHandler(
        ICurrentUserService currentUser,
        UserManager<ApplicationUser> userManager,
        IReadRepository<SubscriptionPlan, int> planReader,
        IPaymentService paymentService)
    {
        _currentUser = currentUser;
        _userManager = userManager;
        _planReader = planReader;
        _paymentService = paymentService;
    }

    public async Task<CheckoutUrlDto> Handle(CreateCheckoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User not found");

        // TODO: Implement proper subscription management (upgrade/downgrade) in the future

        var plan = await _planReader.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan is null || !plan.IsActive)
            throw new NotFoundException("Plan not found or inactive");

        // Select the correct variant ID based on billing interval
        var variantIdStr = request.BillingInterval == Core.Domain.Enums.BillingInterval.Monthly
            ? plan.ExternalPriceIdMonthly
            : plan.ExternalPriceIdYearly;
            
        if (string.IsNullOrWhiteSpace(variantIdStr) || !int.TryParse(variantIdStr, out var variantId))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("BillingInterval", 
                    $"Plan is not configured with Lemon Squeezy variant ID for {request.BillingInterval} billing.")
            });
        }

        var email = user.Email ?? throw new ValidationException(new[]
        {
            new FluentValidation.Results.ValidationFailure("User.Email", "User must have an email to checkout.")
        });

        var name = string.IsNullOrWhiteSpace(user.Name) ? (user.UserName ?? email) : user.Name;

        var checkoutUrl = await _paymentService.CreateCheckoutUrlAsync(
            email,
            name,
            variantId,
            userId,
            null,
            request.SuccessUrl,
            request.CancelUrl,
            cancellationToken);
        return new CheckoutUrlDto { CheckoutUrl = checkoutUrl };
    }
}


