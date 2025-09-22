using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Users.ChangePassword;

public sealed record ChangePasswordCommand(
    string OldPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<Unit>, IAuthenticatedRequest;
