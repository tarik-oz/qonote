using MediatR;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Application.Features.Auth._Shared;

namespace Qonote.Core.Application.Features.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand(
    string Email,
    string Code
) : IRequest<LoginResponseDto>, IEmailBearingRequest;
