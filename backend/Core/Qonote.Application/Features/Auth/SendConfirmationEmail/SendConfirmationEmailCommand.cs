using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Auth.SendConfirmationEmail;

public sealed record SendConfirmationEmailCommand(
    string Email
) : IRequest<SendConfirmationEmailResponse>, IEmailBearingRequest;

public sealed record SendConfirmationEmailResponse(int CooldownSecondsRemaining);
