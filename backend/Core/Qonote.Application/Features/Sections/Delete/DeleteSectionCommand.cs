using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Sections.Delete;

public sealed record DeleteSectionCommand(
    int Id
) : IRequest, IAuthenticatedRequest;
