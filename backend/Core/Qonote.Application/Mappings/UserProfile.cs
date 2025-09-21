using AutoMapper;
using Qonote.Core.Application.Abstractions.Authentication;
using Qonote.Core.Application.Features.Auth.Register;
using Qonote.Core.Application.Features.Users._Shared;
using Qonote.Core.Application.Features.Users.UpdateProfileInfo;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ApplicationUser, UserDto>();

        CreateMap<RegisterUserCommand, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

        CreateMap<ExternalLoginUserDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl));

        CreateMap<UpdateProfileInfoCommand, ApplicationUser>();
    }
}
