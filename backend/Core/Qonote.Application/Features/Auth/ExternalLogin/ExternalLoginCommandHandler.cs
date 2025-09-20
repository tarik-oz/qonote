using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Abstractions.Storage;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.ExternalLogin;

public sealed class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IFileStorageService _fileStorageService;
    private readonly HttpClient _httpClient;

    public ExternalLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IGoogleAuthService googleAuthService,
        IFileStorageService fileStorageService,
        HttpClient httpClient) // Correctly inject HttpClient
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _googleAuthService = googleAuthService;
        _fileStorageService = fileStorageService;
        _httpClient = httpClient;
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
            // User doesn't exist, create a new one
            var newUser = new ApplicationUser
            {
                Email = userInfo.Email,
                UserName = userInfo.Email,
                Name = userInfo.FirstName ?? string.Empty,
                Surname = userInfo.LastName ?? string.Empty,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
                throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));

            // After user is created, they have an ID. Now, download and upload their profile picture.
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
                catch (Exception ex)
                {
                    // Log the exception (ex) but don't fail the whole registration process if image download fails.
                }
            }

            await _userManager.AddToRoleAsync(newUser, "FreeUser");
            user = newUser;
        }

        // This part now correctly executes for both existing and newly created users.
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, accessExpiry) = _tokenService.CreateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiry,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = user.RefreshTokenExpiryTime!.Value
        };
    }
}
