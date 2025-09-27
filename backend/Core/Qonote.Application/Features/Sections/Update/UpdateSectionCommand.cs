using MediatR;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Sections.Update;

public sealed record UpdateSectionCommand(
    int Id,
    string? Title,
    SectionType? Type,
    TimeSpan? StartTime,
    TimeSpan? EndTime
) : IRequest, IAuthenticatedRequest;
