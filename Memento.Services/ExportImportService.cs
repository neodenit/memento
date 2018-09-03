using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Memento.Services
{
    public class ExportImportService : IExportImportService
    {
        private readonly IMementoRepository repository;
        private readonly IConverter converter;
        private readonly IValidator validator;
        private readonly IFactory factory;

        public ExportImportService(IMementoRepository repository, IConverter converter, IValidator validator, IFactory factory)
        {
            this.repository = repository;
            this.converter = converter;
            this.validator = validator;
            this.factory = factory;
        }

        public async Task<string> Export(int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cards = deck.GetAllCards();
            var cardsForExport = from card in cards select converter.FormatForExport(card.Text, card.Comment);
            var result = string.Join(Environment.NewLine, cardsForExport);

            return result;
        }

        public async Task Import(string text, int deckID)
        {
            var cards = converter.GetCardsFromDeck(text);

            foreach (var card in cards)
            {
                var clozeNames = converter.GetClozeNames(card.Item1);
                var isValid = clozeNames.Any() && clozeNames.All(clozeName => validator.Validate(card.Item1, clozeName));
                var deck = await repository.FindDeckAsync(deckID);
                var newCard = factory.CreateCard(deck, card.Item1, card.Item2, isValid);

                repository.AddCard(newCard);

                await repository.SaveChangesAsync();

                if (isValid)
                {
                    await repository.AddClozesAsync(newCard, clozeNames);

                    await repository.SaveChangesAsync();
                }
            }
        }

        public Task<IEnumerable<string>> ConvertApkg(Stream inputStream)
        {
            throw new NotImplementedException();
        }
    }
}

