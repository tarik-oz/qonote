using Qonote.Core.Domain.Common;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Domain.Entities;

public class PromoCodeRedemption : EntityBase<int>
{
    public int PromoCodeId { get; set; }
    public PromoCode? PromoCode { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int UserSubscriptionId { get; set; }
    public UserSubscription? UserSubscription { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
}
