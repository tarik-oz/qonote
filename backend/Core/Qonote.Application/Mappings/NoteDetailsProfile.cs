using AutoMapper;
using Qonote.Core.Application.Features.Notes.GetNoteById;
using Qonote.Core.Domain.Entities;

namespace Qonote.Core.Application.Mappings;

public sealed class NoteDetailsProfile : Profile
{
    public NoteDetailsProfile()
    {
        CreateMap<Block, BlockDto>();

        CreateMap<Section, SectionDto>()
            .ForMember(d => d.Blocks, opt => opt.Ignore());

        CreateMap<Note, NoteDto>()
            .ForMember(d => d.Sections, opt => opt.Ignore());
    }
}
