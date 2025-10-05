using MediatR;
using Qonote.Core.Application.Abstractions.Requests;

namespace Qonote.Core.Application.Features.Sections.DeleteSection;

public sealed record DeleteSectionCommand(
    int Id
) : IRequest, IAuthenticatedRequest;
