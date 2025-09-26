using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Notes.UpdateNote;

public sealed record UpdateNoteCommand(
    int Id,
    string? CustomTitle,
    bool? IsPublic,
    string? UserLink1,
    string? UserLink2,
    string? UserLink3
) : IRequest, IAuthenticatedRequest;
