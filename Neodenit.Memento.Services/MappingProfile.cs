using System;
using System.Linq;
using AutoMapper;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.Common.ViewModels;

namespace Neodenit.Memento.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Deck, DeckViewModel>()
                .ForMember(dest => dest.FirstDelay, opt => opt.MapFrom(src => src.StartDelay))
                .ForMember(dest => dest.SecondDelay, opt => opt.MapFrom(src => (int)Math.Round(src.StartDelay * src.Coeff)))
                .ForMember(dest => dest.CardsCount, opt => opt.MapFrom(src => src.Cards.Count()))
                .ForMember(dest => dest.ValidCardsCount, opt => opt.MapFrom(src => src.ValidCards.Count()));

            CreateMap<Card, ViewCardViewModel>()
                .ForMember(dest => dest.DeckTitle, opt => opt.MapFrom(src => src.Deck.Title));

            CreateMap<Card, EditCardViewModel>();

            CreateMap<Card, AnswerCardViewModel>()
                .ForMember(dest => dest.DeckTitle, opt => opt.MapFrom(src => src.Deck.Title))
                .ForMember(dest => dest.DelayMode, opt => opt.MapFrom(src => src.Deck.DelayMode));

            CreateMap<Cloze, ClozeViewModel>();
        }
    }
}
