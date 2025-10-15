using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Application.Extensions;
using Qonote.Core.Domain.Identity;
using Qonote.Core.Application.Abstractions.Messaging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Domain.Entities;
using Qonote.Core.Domain.Enums;
using Qonote.Core.Application.Common.Subscriptions;

namespace Qonote.Core.Application.Features.Auth.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IReadRepository<SubscriptionPlan, int> _planReader;
    private readonly IWriteRepository<UserSubscription, int> _subWriter;
    private readonly IUnitOfWork _uow;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IEmailTemplateRenderer templateRenderer,
        IMapper mapper,
        ILogger<RegisterUserCommandHandler> logger,
        IReadRepository<SubscriptionPlan, int> planReader,
        IWriteRepository<UserSubscription, int> subWriter,
        IUnitOfWork uow)
    {
        _userManager = userManager;
        _emailService = emailService;
        _templateRenderer = templateRenderer;
        _mapper = mapper;
        _logger = logger;
        _planReader = planReader;
        _subWriter = subWriter;
        _uow = uow;
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

        // Create initial FREE subscription (idempotent best-effort)
        try
        {
            var freePlans = await _planReader.GetAllAsync(p => p.PlanCode == "FREE", cancellationToken);
            var freePlan = freePlans.FirstOrDefault();
            if (freePlan is not null)
            {
                // Simple idempotency: if user already has any subscription, skip creating FREE
                // (Normal register path has none; defensive for retries)
                // We can't query here via repository without a reader; we keep it simple and try insert.
                var now = DateTime.UtcNow;
                var (ps, pe) = SubscriptionPeriodHelper.ComputeContainingPeriod(now, BillingInterval.Monthly, now);
                var freeSub = new UserSubscription
                {
                    UserId = user.Id,
                    PlanId = freePlan.Id,
                    StartDate = now,
                    EndDate = null,
                    Status = SubscriptionStatus.Active,
                    BillingInterval = BillingInterval.Monthly,
                    AutoRenew = false,
                    PaymentProvider = "Free",
                    CurrentPeriodStart = ps,
                    CurrentPeriodEnd = pe
                };
                await _subWriter.AddAsync(freeSub, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Initial FREE subscription created. {UserId}, planId={PlanId}", user.Id, freePlan.Id);
            }
            else
            {
                _logger.LogWarning("FREE plan not found during registration; skipping initial subscription. {UserId}", user.Id);
            }
        }
        catch (Exception ex)
        {
            // Don't fail registration due to free-sub creation hiccup; fallback logic will handle it.
            _logger.LogError(ex, "Failed to create initial FREE subscription. {UserId}", user.Id);
        }

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
