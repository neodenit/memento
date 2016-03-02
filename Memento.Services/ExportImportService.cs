using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Services
{
    public class ExportImportService : IExportImportService
    {
        private readonly IMementoRepository repository;
        private readonly IConverter converter;
        private readonly IValidator validator;

        public ExportImportService(IMementoRepository repository, IConverter converter, IValidator validator)
        {
            this.repository = repository;
            this.converter = converter;
            this.validator = validator;
        }
        
        public async Task<string> Export(int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cards = deck.GetAllCards();
            var cardsForExport = from card in cards select converter.FormatForExport(card.Text);
            var result = string.Join(Environment.NewLine, cardsForExport);

            return result;
        }

        public async Task Import(string text, int deckID)
        {
            var cards = converter.GetCardsFromDeck(text);

            foreach (var cardText in cards)
            {
                var clozeNames = converter.GetClozeNames(cardText);
                var updatedText = converter.ReplaceTextWithWildcards(cardText, clozeNames);
                var isValid = clozeNames.Any() && clozeNames.All(clozeName => validator.Validate(cardText, clozeName));

                if (!isValid)
                {
                    var newCard = new Card
                    {
                        DeckID = deckID,
                        Text = cardText,
                        IsValid = false,
                    };

                    repository.AddCard(newCard);
                }
                else
                {
                    var newCard = new Card
                    {
                        DeckID = deckID,
                        Text = updatedText,
                        IsValid = true,
                    };

                    repository.AddCard(newCard);

                    await repository.SaveChangesAsync();

                    repository.AddClozes(newCard, clozeNames);
                }

                await repository.SaveChangesAsync();
            }
        }
    }
}

