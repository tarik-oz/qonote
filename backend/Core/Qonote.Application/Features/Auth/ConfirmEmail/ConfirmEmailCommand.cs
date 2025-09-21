using MediatR;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommand : IRequest<LoginResponseDto>, IEmailBearingRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
