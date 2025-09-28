using Microsoft.EntityFrameworkCore;
using Qonote.Core.Application.Abstractions.Subscriptions;
using Qonote.Core.Application.Exceptions;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Subscriptions;

public class LimitCheckerService : ILimitCheckerService
{
    private readonly ApplicationDbContext _db;
    private readonly IPlanResolver _planResolver;

    public LimitCheckerService(ApplicationDbContext db, IPlanResolver planResolver)
    {
        _db = db;
        _planResolver = planResolver;
    }

    public async Task EnsureUserCanCreateNoteAsync(string userId, CancellationToken cancellationToken = default)
    {
        var plan = await _planResolver.GetEffectivePlanAsync(userId, cancellationToken);

        // Count active notes for this user (IsDeleted = false)
        var activeCount = await _db.Notes.AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .CountAsync(cancellationToken);

        if (activeCount >= plan.MaxNoteCount)
        {
            var failures = new[]
            {
                new FluentValidation.Results.ValidationFailure("Limit.MaxNoteCount", $"You reached the note limit for your plan ({plan.PlanCode}).")
            };
            throw new ValidationException(failures);
        }
    }
}
