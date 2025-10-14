using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Features.Subscriptions.RedeemPromoCode;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Rules.PromoCodes;

public sealed class SingleUsePerUserRule : IBusinessRule<RedeemPromoCodeCommand>
{
    public int Order => 30;
    private readonly IReadRepository<PromoCode, int> _promoRead;
    private readonly IReadRepository<PromoCodeRedemption, int> _redemptionRead;
    private readonly ICurrentUserService _currentUser;

    public SingleUsePerUserRule(
        IReadRepository<PromoCode, int> promoRead,
        IReadRepository<PromoCodeRedemption, int> redemptionRead,
        ICurrentUserService currentUser)
    { _promoRead = promoRead; _redemptionRead = redemptionRead; _currentUser = currentUser; }

    public async Task<IEnumerable<RuleViolation>> CheckAsync(RedeemPromoCodeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return new[] { new RuleViolation("User", "NotAuthenticated") };

        var code = request.Code.Trim().ToUpperInvariant();
        var promoList = await _promoRead.GetAllAsync(p => p.Code == code, cancellationToken);
        var promo = promoList.FirstOrDefault();
        if (promo is null) return Array.Empty<RuleViolation>(); // handled by state rule
        if (!promo.SingleUsePerUser) return Array.Empty<RuleViolation>();

        var redemptions = await _redemptionRead.GetAllAsync(r => r.PromoCodeId == promo.Id && r.UserId == userId, cancellationToken);
        if (redemptions.Any())
            return new[] { new RuleViolation("PromoCode", "AlreadyRedeemed") };
        return Array.Empty<RuleViolation>();
    }
}
