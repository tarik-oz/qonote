using FluentValidation;

namespace Qonote.Core.Application.Features.NoteGroups.ListGroupNotes;

public sealed class ListGroupNotesQueryValidator : AbstractValidator<ListGroupNotesQuery>
{
    private const int MaxLimit = 100;

    public ListGroupNotesQueryValidator()
    {
        RuleFor(x => x.GroupId).GreaterThan(0);

        When(x => x.Offset.HasValue, () =>
        {
            RuleFor(x => x.Offset!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Offset must be >= 0.");
        });

        When(x => x.Limit.HasValue, () =>
        {
            RuleFor(x => x.Limit!.Value)
                .GreaterThan(0).WithMessage("Limit must be > 0.")
                .LessThanOrEqualTo(MaxLimit).WithMessage($"Limit must be <= {MaxLimit}.");
        });
    }
}
