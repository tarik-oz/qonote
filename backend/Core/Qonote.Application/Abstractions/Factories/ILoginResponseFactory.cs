using Qonote.Core.Application.Features.Auth._Shared;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Abstractions.Factories;

public interface ILoginResponseFactory
{
    Task<LoginResponseDto> CreateAsync(ApplicationUser user);
}
