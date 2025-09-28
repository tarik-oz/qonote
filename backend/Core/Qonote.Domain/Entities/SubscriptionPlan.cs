using Qonote.Core.Domain.Common;

namespace Qonote.Core.Domain.Entities;

public class SubscriptionPlan : EntityBase<int>
{
    public string PlanCode { get; set; } = string.Empty; // e.g., FREE, PREMIUM, PREMIUM_PLUS
    public string Name { get; set; } = string.Empty; // display name
    public int MaxNoteCount { get; set; }
}
