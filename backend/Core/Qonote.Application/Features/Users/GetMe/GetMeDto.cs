namespace Qonote.Core.Application.Features.Users.GetMe;

public sealed record GetMeDto(
    string Id,
    string Email,
    string Name,
    string Surname,
    string? ProfileImageUrl,
    string PlanCode
);
