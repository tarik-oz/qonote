using MediatR;

namespace Qonote.Core.Application.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email
) : IRequest<Unit>;
