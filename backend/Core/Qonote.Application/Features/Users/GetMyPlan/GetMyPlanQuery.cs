using MediatR;

namespace Qonote.Core.Application.Features.Users.GetMyPlan;

public sealed record GetMyPlanQuery : IRequest<MyPlanDto>;
