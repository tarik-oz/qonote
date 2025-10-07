using MediatR;
using Qonote.Core.Application.Features.Subscriptions._Shared;

namespace Qonote.Core.Application.Features.Subscriptions.ListMyPayments;

public sealed record ListMyPaymentsQuery() : IRequest<List<PaymentDto>>;


