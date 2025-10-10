using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Blocks.UpdateBlock;

public sealed class UpdateBlockCommandHandler : IRequestHandler<UpdateBlockCommand>
{
    private readonly IReadRepository<Block, Guid> _blockReader;
    private readonly IReadRepository<Section, int> _sectionReader;
    private readonly IReadRepository<Note, int> _noteReader;
    private readonly IWriteRepository<Block, Guid> _blockWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateBlockCommandHandler> _logger;

    public UpdateBlockCommandHandler(
        IReadRepository<Block, Guid> blockReader,
        IReadRepository<Section, int> sectionReader,
        IReadRepository<Note, int> noteReader,
        IWriteRepository<Block, Guid> blockWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<UpdateBlockCommandHandler> logger)
    {
        _blockReader = blockReader;
        _sectionReader = sectionReader;
        _noteReader = noteReader;
        _blockWriter = blockWriter;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(UpdateBlockCommand request, CancellationToken cancellationToken)
    {
        var block = await _blockReader.GetByIdAsync(request.Id, cancellationToken);
        if (block is null)
        {
            throw new NotFoundException($"Block {request.Id} not found.");
        }

        var section = await _sectionReader.GetByIdAsync(block.SectionId, cancellationToken);
        if (section is null)
        {
            throw new NotFoundException($"Block {request.Id} not found.");
        }

        var userId = _currentUser.UserId!;
        var note = await _noteReader.GetByIdAsync(section.NoteId, cancellationToken);
        if (note is null || note.UserId != userId)
        {
            throw new NotFoundException($"Block {request.Id} not found.");
        }

        if (request.Content is not null) block.Content = request.Content;
        if (request.Type is not null) block.Type = request.Type.Value;

        _blockWriter.Update(block);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Block updated. BlockId={BlockId} UserId={UserId}", block.Id, userId);
    }
}
