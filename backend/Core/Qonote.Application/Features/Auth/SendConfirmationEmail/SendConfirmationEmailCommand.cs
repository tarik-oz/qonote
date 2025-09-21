using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Auth.SendConfirmationEmail;

public sealed class SendConfirmationEmailCommand : IRequest<Unit>, IEmailBearingRequest
{
    public string Email { get; set; } = string.Empty;
}
