using MediatR;

namespace Qonote.Core.Application.Features.Subscriptions.ResumeMySubscription;

public sealed record ResumeMySubscriptionCommand() : IRequest<Unit>;


