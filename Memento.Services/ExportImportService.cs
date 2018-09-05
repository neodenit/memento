using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Memento.Interfaces;

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
            var cards = deck.GetValidCards();
            var cardsForExport = from card in cards select converter.FormatForExport(card.Text, card.Comment);
            var result = string.Join(Environment.NewLine, cardsForExport);

            return result;
        }

        public async Task Import(string deckText, int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cardTextComments = converter.GetCardsFromDeck(deckText).ToList();

            foreach (var cardTextComment in cardTextComments)
            {
                var clozeNames = converter.GetClozeNames(cardTextComment.Item1);
                var isValid = clozeNames.Any() && clozeNames.All(clozeName => validator.Validate(cardTextComment.Item1, clozeName));
                var newCard = factory.CreateCard(deck, cardTextComment.Item1, cardTextComment.Item2, isValid);

                if (!string.IsNullOrWhiteSpace(newCard.Text))
                {
                    repository.AddCard(newCard);

                    await repository.SaveChangesAsync();

                    if (isValid)
                    {
                        await repository.AddClozesAsync(newCard, clozeNames);
                    }
                }
            }

            await repository.SaveChangesAsync();
        }

        public Task<IEnumerable<string>> ConvertApkg(Stream inputStream)
        {
            throw new NotImplementedException();
        }
    }
}

