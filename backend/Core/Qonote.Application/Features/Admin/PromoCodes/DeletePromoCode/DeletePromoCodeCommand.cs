using MediatR;

namespace Qonote.Core.Application.Features.Admin.PromoCodes.DeletePromoCode;

public sealed record DeletePromoCodeCommand(
    int Id
) : IRequest;
