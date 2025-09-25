using MediatR;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Security;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.NoteGroups.CreateNoteGroup;

public sealed class CreateNoteGroupCommandHandler : IRequestHandler<CreateNoteGroupCommand, int>
{
    private readonly IWriteRepository<NoteGroup, int> _groupWriter;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateNoteGroupCommandHandler(
        IWriteRepository<NoteGroup, int> groupWriter,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _groupWriter = groupWriter;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateNoteGroupCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!; // guaranteed by UserMustExistRule

        var group = new NoteGroup
        {
            Name = request.Title.Trim(),
            UserId = userId
        };

        await _groupWriter.AddAsync(group, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return group.Id;
    }
}
