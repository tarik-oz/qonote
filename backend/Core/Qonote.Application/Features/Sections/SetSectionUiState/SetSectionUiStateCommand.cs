using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Sections.SetSectionUiState;

public sealed record SetSectionUiStateCommand(
    int SectionId,
    bool IsCollapsed
) : IRequest, IAuthenticatedRequest;
