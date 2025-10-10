using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Authentication;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.Media;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ExternalLogin;

public sealed class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoginResponseFactory _loginResponseFactory;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IImageService _imageService;
    private readonly IMapper _mapper;
    private readonly ILogger<ExternalLoginCommandHandler> _logger;

    public ExternalLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILoginResponseFactory loginResponseFactory,
        IGoogleAuthService googleAuthService,
        IImageService imageService,
        IMapper mapper,
        ILogger<ExternalLoginCommandHandler> logger)
    {
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
        _googleAuthService = googleAuthService;
        _imageService = imageService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        var provider = request.Provider.Trim();

        // Use configured RedirectUri from settings when not provided explicitly
        var userInfo = await _googleAuthService.ExchangeCodeForUserInfoAsync(code, redirectUri: string.Empty);

        var user = await _userManager.FindByEmailAsync(userInfo.Email.Trim());
        if (user is null)
        {
            var newUser = _mapper.Map<ApplicationUser>(userInfo);
            newUser.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
                throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));

            if (!string.IsNullOrWhiteSpace(userInfo.ProfilePictureUrl))
            {
                var uploadedUrl = await _imageService.UploadProfilePictureFromUrlAsync(userInfo.ProfilePictureUrl, newUser.Id, cancellationToken);
                if (!string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    newUser.ProfileImageUrl = uploadedUrl!;
                    await _userManager.UpdateAsync(newUser);
                }
            }

            await _userManager.AddToRoleAsync(newUser, "FreeUser");
            user = newUser;
        }

        var response = await _loginResponseFactory.CreateAsync(user);
        _logger.LogInformation("External login succeeded. UserId={UserId} Provider={Provider}", user.Id, provider);
        return response;
    }
}
