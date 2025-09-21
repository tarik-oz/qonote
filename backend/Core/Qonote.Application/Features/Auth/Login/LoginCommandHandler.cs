using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoginResponseFactory _loginResponseFactory;

    public LoginCommandHandler(UserManager<ApplicationUser> userManager, ILoginResponseFactory loginResponseFactory)
    {
        _userManager = userManager;
        _loginResponseFactory = loginResponseFactory;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Auth.Login", "Invalid email or password.") });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Auth.Login", "Invalid email or password.") });
        }

        return await _loginResponseFactory.CreateAsync(user);
    }
}
