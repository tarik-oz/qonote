using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;
using FluentValidation.Results;

namespace Qonote.Core.Application.Features.Subscriptions.RedeemPromoCode;

public sealed class RedeemPromoCodeCommandHandler : IRequestHandler<RedeemPromoCodeCommand, int>
{
    private readonly IReadRepository<PromoCode, int> _promoRead;
    private readonly IWriteRepository<PromoCode, int> _promoWrite;
    private readonly IReadRepository<UserSubscription, int> _subRead;
    private readonly IWriteRepository<UserSubscription, int> _subWrite;
    private readonly IWriteRepository<PromoCodeRedemption, int> _redemptionWrite;
    private readonly IWriteRepository<Payment, int> _paymentWrite;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<RedeemPromoCodeCommandHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public RedeemPromoCodeCommandHandler(
        IReadRepository<PromoCode, int> promoRead,
        IWriteRepository<PromoCode, int> promoWrite,
        IReadRepository<UserSubscription, int> subRead,
        IWriteRepository<UserSubscription, int> subWrite,
        IWriteRepository<PromoCodeRedemption, int> redemptionWrite,
        IWriteRepository<Payment, int> paymentWrite,
        IUnitOfWork uow,
        ILogger<RedeemPromoCodeCommandHandler> logger,
        ICurrentUserService currentUser)
    {
        _promoRead = promoRead;
        _promoWrite = promoWrite;
        _subRead = subRead;
        _subWrite = subWrite;
        _redemptionWrite = redemptionWrite;
        _paymentWrite = paymentWrite;
        _uow = uow;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(RedeemPromoCodeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            throw new ValidationException(new[] { new ValidationFailure("User", "User must be authenticated") });

        var code = request.Code.Trim().ToUpperInvariant();
        _logger.LogInformation("Redeem promo attempt userId={UserId} code={Code}", userId, code);

        var promoList = await _promoRead.GetAllAsync(p => p.Code == code, cancellationToken);
        var promo = promoList.FirstOrDefault();
        if (promo is null || promo.IsDeleted)
            throw new NotFoundException("Promo code not found");
        if (!promo.IsActive)
            throw new ConflictException("Promo code inactive");
        if (promo.ExpiresAt.HasValue && promo.ExpiresAt.Value <= DateTime.UtcNow)
            throw new ConflictException("Promo code expired");
        if (promo.MaxRedemptions.HasValue && promo.RedemptionCount >= promo.MaxRedemptions.Value)
            throw new ConflictException("Promo code redemption limit reached");

        var existingSubs = await _subRead.GetAllAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active, cancellationToken);
        var hasActive = existingSubs.Any(s => !s.EndDate.HasValue || s.EndDate.Value > DateTime.UtcNow);
        if (hasActive)
            throw new ConflictException("User already has an active subscription");

        const int maxAttempts = 3;
        int attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                if (attempt > 1)
                {
                    promoList = await _promoRead.GetAllAsync(p => p.Code == code, cancellationToken);
                    promo = promoList.FirstOrDefault();
                    if (promo is null)
                        throw new NotFoundException("Promo code not found");
                    if (!promo.IsActive || (promo.ExpiresAt.HasValue && promo.ExpiresAt.Value <= DateTime.UtcNow))
                        throw new ConflictException("Promo code no longer redeemable");
                    if (promo.MaxRedemptions.HasValue && promo.RedemptionCount >= promo.MaxRedemptions.Value)
                        throw new ConflictException("Promo code redemption limit reached");
                }

                var now = DateTime.UtcNow;
                var start = now;
                var duration = promo!.DurationMonths <= 0 ? 1 : promo.DurationMonths;
                var end = start.AddMonths(duration);

                var userSub = new UserSubscription
                {
                    UserId = userId!,
                    PlanId = promo.PlanId,
                    StartDate = start,
                    EndDate = end,
                    PriceAmount = 0m,
                    Currency = "USD",
                    BillingInterval = BillingInterval.Monthly,
                    Status = SubscriptionStatus.Active,
                    PaymentProvider = "PromoCode",
                    AutoRenew = false,
                    CurrentPeriodStart = start,
                    CurrentPeriodEnd = end
                };
                await _subWrite.AddAsync(userSub, cancellationToken);

                var redemption = new PromoCodeRedemption
                {
                    PromoCodeId = promo.Id,
                    UserId = userId!,
                    UserSubscriptionId = userSub.Id,
                    RedeemedAt = now
                };
                await _redemptionWrite.AddAsync(redemption, cancellationToken);

                var payment = new Payment
                {
                    UserId = userId!,
                    UserSubscriptionId = userSub.Id,
                    Amount = 0m,
                    Currency = "USD",
                    Status = PaymentStatus.Succeeded,
                    PaymentProvider = "PromoCode",
                    PaidAt = now,
                    Description = $"Promo code {promo.Code} redemption"
                };
                await _paymentWrite.AddAsync(payment, cancellationToken);

                promo.RedemptionCount += 1;
                _promoWrite.Update(promo);

                await _uow.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Promo redeemed userId={UserId} code={Code} subscriptionId={SubId} attempt={Attempt}", userId, promo.Code, userSub.Id, attempt);
                return userSub.Id;
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "Promo redeem concurrency retry userId={UserId} code={Code} attempt={Attempt}", userId, code, attempt);
                await Task.Delay(50 * attempt, cancellationToken);
                continue;
            }
        }
    }
}
