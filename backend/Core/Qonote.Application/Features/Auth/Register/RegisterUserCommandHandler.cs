using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Extensions;
using Qonote.Core.Domain.Identity;
using Qonote.Core.Application.Abstractions.Messaging;

namespace Qonote.Core.Application.Features.Auth.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IEmailTemplateRenderer templateRenderer,
        IMapper mapper,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _templateRenderer = templateRenderer;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Unit> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Note: The EmailMustBeUniqueRule is handled by the BusinessRulesBehavior

        // Normalize incoming strings (trim) to persist clean data
        var normalized = request with
        {
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            Email = request.Email.Trim()
        };

        var user = _mapper.Map<ApplicationUser>(normalized);
        user.EmailConfirmed = false; // Set non-mapped properties

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            // Log the failure without PII and throw a validation exception to be handled by middleware
            var errors = result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)).ToList();
            _logger.LogWarning("User registration failed: {Errors}", string.Join(", ", errors.Select(e => e.ErrorMessage)));
            throw new ValidationException(errors);
        }

        _logger.LogInformation("User created successfully. Generating confirmation code. {UserId}", user.Id);

        // Generate code, update user, and get the code back
        var (confirmationCode, _) = await _userManager.GenerateAndSetEmailConfirmationCodeAsync(user);

        // Prepare confirmation email from template
        var emailBody = await _templateRenderer.RenderAsync("RegistrationConfirmation", new Dictionary<string, string>
        {
            ["name"] = user.Name,
            ["code"] = confirmationCode
        });

        try
        {
            await _emailService.SendEmailAsync(user.Email!, "Confirm your Qonote Account", emailBody);
            _logger.LogInformation("Confirmation email sent. {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            // Log the email sending failure but don't fail the entire registration process.
            // The user can request a new confirmation email later.
            _logger.LogError(ex, "Failed to send confirmation email during registration. {UserId}", user.Id);
        }

        return Unit.Value;
    }
}
