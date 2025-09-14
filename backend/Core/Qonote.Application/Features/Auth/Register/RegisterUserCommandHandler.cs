using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            // Map Identity errors into ValidationException to keep API errors consistent
            throw new Qonote.Core.Application.Exceptions.ValidationException(result.Errors
                .Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
        }

        return $"User {user.Name} {user.Surname} registered successfully.";
    }
}
