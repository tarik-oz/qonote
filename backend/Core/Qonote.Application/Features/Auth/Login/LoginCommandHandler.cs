using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Factories;
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
        // All business rules (user existence, password validity, email confirmation) 
        // are handled by the BusinessRulesBehavior.
        var user = await _userManager.FindByEmailAsync(request.Email);

        return await _loginResponseFactory.CreateAsync(user!);
    }
}
