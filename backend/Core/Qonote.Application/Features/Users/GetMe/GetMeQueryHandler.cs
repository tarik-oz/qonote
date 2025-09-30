using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Users.GetMe;

public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, GetMeDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPlanResolver _planResolver;

    public GetMeQueryHandler(
        ICurrentUserService currentUser,
        UserManager<ApplicationUser> userManager,
        IPlanResolver planResolver)
    {
        _currentUser = currentUser;
        _userManager = userManager;
        _planResolver = planResolver;
    }

    public async Task<GetMeDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
            throw new NotFoundException("User not found.");

        var effective = await _planResolver.GetEffectivePlanAsync(userId, cancellationToken);
        return new GetMeDto(user.Id, user.Email!, user.Name, user.Surname, user.ProfileImageUrl, effective.PlanCode);
    }
}
