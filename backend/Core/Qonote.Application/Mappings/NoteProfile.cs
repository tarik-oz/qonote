using AutoMapper;
using Qonote.Core.Application.Abstractions.YouTube.Models;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Mappings;

public sealed class NoteProfile : Profile
{
    public NoteProfile()
    {
        CreateMap<YouTubeVideoMetadata, Note>()
            .ForMember(d => d.VideoTitle, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.ChannelName, opt => opt.MapFrom(s => s.ChannelTitle))
            .ForMember(d => d.ThumbnailUrl, opt => opt.MapFrom(s => s.ThumbnailUrl))
            .ForMember(d => d.VideoDuration, opt => opt.MapFrom(s => s.Duration))
            .ForMember(d => d.CustomTitle, opt => opt.Ignore())
            .ForMember(d => d.YoutubeUrl, opt => opt.Ignore())
            .ForMember(d => d.UserId, opt => opt.Ignore())
            .ForMember(d => d.NoteGroupId, opt => opt.Ignore())
            .ForMember(d => d.Sections, opt => opt.Ignore());
    }
}
