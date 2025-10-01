using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Identity;
using Qonote.Core.Application.Abstractions.Caching;

namespace Qonote.Core.Application.Features.Users.GetMe;

public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, GetMeDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPlanResolver _planResolver;
    private readonly ICacheService _cache;
    private readonly ICacheTtlProvider _ttl;

    public GetMeQueryHandler(
        ICurrentUserService currentUser,
        UserManager<ApplicationUser> userManager,
        IPlanResolver planResolver,
        ICacheService cache,
        ICacheTtlProvider ttl)
    {
        _currentUser = currentUser;
        _userManager = userManager;
        _planResolver = planResolver;
        _cache = cache;
        _ttl = ttl;
    }

    public async Task<GetMeDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var key = $"qonote:me:{userId}";
        var me = await _cache.GetOrSetAsync(key, async ct =>
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || user.IsDeleted)
                throw new NotFoundException("User not found.");

            var effective = await _planResolver.GetEffectivePlanAsync(userId, ct);
            return new GetMeDto(user.Id, user.Email!, user.Name, user.Surname, user.ProfileImageUrl, effective.PlanCode);
        }, _ttl.GetMeTtl(), cancellationToken);

        return me!;
    }
}
