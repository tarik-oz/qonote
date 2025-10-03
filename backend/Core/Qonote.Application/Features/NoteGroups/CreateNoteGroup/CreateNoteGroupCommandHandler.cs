using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.NoteGroups.CreateNoteGroup;

public sealed class CreateNoteGroupCommandHandler : IRequestHandler<CreateNoteGroupCommand, int>
{
    private readonly IWriteRepository<NoteGroup, int> _groupWriter;
    private readonly IReadRepository<NoteGroup, int> _groupReader;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateNoteGroupCommandHandler(
        IWriteRepository<NoteGroup, int> groupWriter,
        IReadRepository<NoteGroup, int> groupReader,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _groupWriter = groupWriter;
        _groupReader = groupReader;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateNoteGroupCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule

        // Set Order: new groups go to the end
        var existingGroups = await _groupReader.GetAllAsync(g => g.UserId == userId, cancellationToken);
        var newOrder = existingGroups.Any() ? existingGroups.Max(g => g.Order) + 1 : 0;

        var group = new NoteGroup
        {
            Name = request.Title.Trim(),
            UserId = userId,
            Order = newOrder
        };

        await _groupWriter.AddAsync(group, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return group.Id;
    }
}
