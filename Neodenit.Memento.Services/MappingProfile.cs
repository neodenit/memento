using AutoMapper;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Deck, DeckViewModel>();
            CreateMap<Card, ViewCardViewModel>();
            CreateMap<Card, EditCardViewModel>();
            CreateMap<Cloze, ClozeViewModel>();
        }
    }
}
