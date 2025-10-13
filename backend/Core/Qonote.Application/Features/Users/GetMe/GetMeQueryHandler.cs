using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Identity;
using Qonote.Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Configuration;
using Qonote.Core.Application.Abstractions.Storage;

namespace Qonote.Core.Application.Features.Users.GetMe;

public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, GetMeDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPlanResolver _planResolver;
    private readonly ICacheService _cache;
    private readonly ICacheTtlProvider _ttl;
    private readonly IConfiguration _configuration;
    private readonly IFileReadUrlService _readUrlService;

    public GetMeQueryHandler(
        ICurrentUserService currentUser,
        UserManager<ApplicationUser> userManager,
        IPlanResolver planResolver,
        ICacheService cache,
        ICacheTtlProvider ttl,
        IConfiguration configuration,
        IFileReadUrlService readUrlService)
    {
        _currentUser = currentUser;
        _userManager = userManager;
        _planResolver = planResolver;
        _cache = cache;
        _ttl = ttl;
        _configuration = configuration;
        _readUrlService = readUrlService;
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
            var defaultUrl = _configuration["Profile:DefaultProfileImageUrl"];
            var rawProfileUrl = string.IsNullOrWhiteSpace(user.ProfileImageUrl) ? defaultUrl : user.ProfileImageUrl;
            // Optionally wrap with a time-limited read URL (SAS) if storage supports it.
            var ttlStr = _configuration["Profile:ProfileImageUrlTtlSeconds"]; // optional
            int ttlSeconds = 0;
            _ = int.TryParse(ttlStr, out ttlSeconds);
            var finalProfileUrl = rawProfileUrl ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(rawProfileUrl) && ttlSeconds > 0)
            {
                finalProfileUrl = await _readUrlService.GetReadUrlAsync(rawProfileUrl!, TimeSpan.FromSeconds(ttlSeconds), ct);
            }
            return new GetMeDto(user.Id, user.Email!, user.Name, user.Surname, finalProfileUrl, effective.PlanCode);
        }, _ttl.GetMeTtl(), cancellationToken);

        return me!;
    }
}
