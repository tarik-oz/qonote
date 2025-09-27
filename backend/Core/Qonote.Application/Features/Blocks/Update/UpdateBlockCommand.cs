using MediatR;
using Qonote.Core.Application.Abstractions.Requests;
using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Features.Blocks.Update;

public sealed record UpdateBlockCommand(
    Guid Id,
    string? Content,
    BlockType? Type
) : IRequest, IAuthenticatedRequest;
