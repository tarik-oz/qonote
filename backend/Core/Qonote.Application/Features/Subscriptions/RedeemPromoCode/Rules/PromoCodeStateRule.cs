using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Subscriptions.RedeemPromoCode.Rules;

public sealed class PromoCodeStateRule : IBusinessRule<RedeemPromoCodeCommand>
{
    public int Order => 10;
    private readonly IReadRepository<PromoCode, int> _promoRead;
    public PromoCodeStateRule(IReadRepository<PromoCode, int> promoRead) => _promoRead = promoRead;

    public async Task<IEnumerable<RuleViolation>> CheckAsync(RedeemPromoCodeCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var list = await _promoRead.GetAllAsync(p => p.Code == code, cancellationToken);
        var promo = list.FirstOrDefault();
        if (promo is null || promo.IsDeleted)
            return [new RuleViolation("PromoCode", "Promo code not found")];
        var violations = new List<RuleViolation>();
        if (!promo.IsActive) violations.Add(new RuleViolation("PromoCode", "Inactive"));
        if (promo.ExpiresAt.HasValue && promo.ExpiresAt.Value <= DateTime.UtcNow) violations.Add(new RuleViolation("PromoCode", "Expired"));
        if (promo.MaxRedemptions.HasValue && promo.RedemptionCount >= promo.MaxRedemptions.Value) violations.Add(new RuleViolation("PromoCode", "LimitReached"));
        return violations;
    }
}
