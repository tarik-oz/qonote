using MediatR;
using Microsoft.Extensions.Logging;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Exceptions;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.DeletePromoCode;

public sealed class DeletePromoCodeCommandHandler : IRequestHandler<DeletePromoCodeCommand>
{
    private readonly IReadRepository<PromoCode, int> _read;
    private readonly IWriteRepository<PromoCode, int> _write;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DeletePromoCodeCommandHandler> _logger;

    public DeletePromoCodeCommandHandler(
        IReadRepository<PromoCode, int> read,
        IWriteRepository<PromoCode, int> write,
        IUnitOfWork uow,
        ILogger<DeletePromoCodeCommandHandler> logger)
    {
        _read = read;
        _write = write;
        _uow = uow;
        _logger = logger;
    }

    public async Task Handle(DeletePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var list = await _read.GetAllAsync(p => p.Id == request.Id, cancellationToken);
        var entity = list.FirstOrDefault();
        if (entity is null)
            throw new NotFoundException($"Promo code {request.Id} not found");

        if (entity.IsDeleted)
        {
            _logger.LogWarning("Promo code already deleted id={Id}", entity.Id);
            return;
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _write.Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Promo code soft-deleted id={Id}", entity.Id);
    }
}
