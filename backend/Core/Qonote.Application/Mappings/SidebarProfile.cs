using AutoMapper;
using Qonote.Core.Application.Features.Sidebar.GetSidebar;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Mappings;

public sealed class SidebarProfile : Profile
{
    public SidebarProfile()
    {
        // Note -> NoteListItemDto
        CreateMap<Note, NoteListItemDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => string.IsNullOrEmpty(s.CustomTitle) ? s.VideoTitle : s.CustomTitle))
            .ForMember(d => d.ThumbnailUrl, opt => opt.MapFrom(s => s.ThumbnailUrl))
            .ForMember(d => d.Order, opt => opt.MapFrom(s => s.Order));

        // NoteGroup -> GroupItemDto (computed fields set in handler)
        CreateMap<NoteGroup, GroupItemDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Order, opt => opt.MapFrom(s => s.Order))
            .ForMember(d => d.NoteCount, opt => opt.Ignore())
            .ForMember(d => d.PreviewThumbnails, opt => opt.Ignore());
    }
}
