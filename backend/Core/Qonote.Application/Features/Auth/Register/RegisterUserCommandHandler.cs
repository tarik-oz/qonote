using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Qonote.Core.Application.Abstractions.Messaging;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Extensions;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Features.Auth.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService, IMapper mapper)
    {
        _userManager = userManager;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Note: The EmailMustBeUniqueRule is handled by the BusinessRulesBehavior

        var user = _mapper.Map<ApplicationUser>(request);
        user.EmailConfirmed = false; // Set non-mapped properties

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
        }

        // Generate code, update user, and get the code back
        var (confirmationCode, _) = await _userManager.GenerateAndSetEmailConfirmationCodeAsync(user);

        // Send confirmation email
        var emailBody = $"<h1>Welcome to Qonote!</h1><p>Your confirmation code is: <strong>{confirmationCode}</strong></p>";
        await _emailService.SendEmailAsync(user.Email!, "Confirm your Qonote Account", emailBody);

        return Unit.Value;
    }
}
