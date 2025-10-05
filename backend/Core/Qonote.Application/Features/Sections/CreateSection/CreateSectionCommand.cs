using MediatR;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Sections.CreateSection;

public sealed record CreateSectionCommand(
    int NoteId,
    string? Title,
    SectionType? Type,
    TimeSpan? StartTime,
    TimeSpan? EndTime
) : IRequest<int>, IAuthenticatedRequest;
