using Ardalis.Result;
using MediatR;

namespace RespectLedger.Application.Features.Users.Queries.GetManaBalance;

public record GetManaBalanceQuery(Guid UserId) : IRequest<Result<ManaBalanceDto>>;
