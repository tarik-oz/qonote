using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Abstractions.Rules.Models;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Subscriptions.RedeemPromoCode.Rules;

public sealed class UserNoActiveSubscriptionRule : IBusinessRule<RedeemPromoCodeCommand>
{
    public int Order => 20;
    private readonly IReadRepository<UserSubscription, int> _subRead;
    private readonly ICurrentUserService _currentUser;
    public UserNoActiveSubscriptionRule(IReadRepository<UserSubscription, int> subRead, ICurrentUserService currentUser)
    { _subRead = subRead; _currentUser = currentUser; }

    public async Task<IEnumerable<RuleViolation>> CheckAsync(RedeemPromoCodeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId)) return [new RuleViolation("User", "NotAuthenticated")];
        var subs = await _subRead.GetAllAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active, cancellationToken);
        if (subs.Any(s => !s.EndDate.HasValue || s.EndDate.Value > DateTime.UtcNow))
            return [new RuleViolation("Subscription", "ActiveExists")];
        return Array.Empty<RuleViolation>();
    }
}
