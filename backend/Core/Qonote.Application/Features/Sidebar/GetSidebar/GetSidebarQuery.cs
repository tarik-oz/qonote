using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Sidebar.GetSidebar;

public sealed record GetSidebarQuery(
    int? Limit = null,
    int? Offset = null
) : IRequest<SidebarDto>, IAuthenticatedRequest;
