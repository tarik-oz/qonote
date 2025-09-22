using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Authentication;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.Storage;
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
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;

    public ExternalLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILoginResponseFactory loginResponseFactory,
        IGoogleAuthService googleAuthService,
        IFileStorageService fileStorageService,
        HttpClient httpClient,
        IMapper mapper)
    {
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
        _googleAuthService = googleAuthService;
        _fileStorageService = fileStorageService;
        _httpClient = httpClient;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        if (request.Provider.ToLower() != "google")
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Provider", "Only Google provider is supported.") });
        }

        const string redirectUri = "https://developers.google.com/oauthplayground";
        var userInfo = await _googleAuthService.ExchangeCodeForUserInfoAsync(request.Code, redirectUri);

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
                try
                {
                    var newFileName = newUser.Id + ".jpg";
                    var response = await _httpClient.GetAsync(userInfo.ProfilePictureUrl, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                        var uploadedUrl = await _fileStorageService.UploadAsync(stream, "profile-pictures", newFileName, contentType);
                        newUser.ProfileImageUrl = uploadedUrl;
                        await _userManager.UpdateAsync(newUser);
                    }
                }
                catch (Exception)
                {
                    // Log the exception (ex) but don't fail the registration process if image download fails.
                }
            }

            await _userManager.AddToRoleAsync(newUser, "FreeUser");
            user = newUser;
        }

        return await _loginResponseFactory.CreateAsync(user);
    }
}
