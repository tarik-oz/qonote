using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Authentication;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.Storage;
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
    private readonly IFileStorageService _fileStorageService;
    private readonly IImageService _imageService;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;

    public ExternalLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILoginResponseFactory loginResponseFactory,
        IGoogleAuthService googleAuthService,
        IFileStorageService fileStorageService,
        IImageService imageService,
        HttpClient httpClient,
        IMapper mapper)
    {
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
        _googleAuthService = googleAuthService;
        _fileStorageService = fileStorageService;
        _imageService = imageService;
        _httpClient = httpClient;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        if (request.Provider.ToLower() != "google")
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Provider", "Only Google provider is supported.") });
        }

        // Use configured RedirectUri from settings when not provided explicitly
        var userInfo = await _googleAuthService.ExchangeCodeForUserInfoAsync(request.Code, redirectUri: string.Empty);

        var user = await _userManager.FindByEmailAsync(userInfo.Email);
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

        return await _loginResponseFactory.CreateAsync(user);
    }
}
