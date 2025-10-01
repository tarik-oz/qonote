using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Caching;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Users.UpdateProfileInfo;

public sealed class UpdateProfileInfoCommandHandler : IRequestHandler<UpdateProfileInfoCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ICacheInvalidationService _cacheInvalidation;

    public UpdateProfileInfoCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService, IMapper mapper, ICacheInvalidationService cacheInvalidation)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _cacheInvalidation = cacheInvalidation;
    }

    public async Task<Unit> Handle(UpdateProfileInfoCommand request, CancellationToken cancellationToken)
    {
        // The UserMustExistRule is handled by the BusinessRulesBehavior.
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId!);

        // Map properties from the command onto the existing user entity
        _mapper.Map(request, user!);

        var result = await _userManager.UpdateAsync(user!);

        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
        }

        // Invalidate /api/me cache so next fetch reflects updated info
        await _cacheInvalidation.RemoveMeAsync(_currentUserService.UserId!, cancellationToken);

        return Unit.Value;
    }
}
