using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Memento.Interfaces;
using Memento.Models.Models;
using Newtonsoft.Json;

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

        public async Task<string> Export(Guid deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cards = deck.GetValidCards();
            var cardsForExport = from card in cards select converter.FormatForExport(card.Text, card.Comment);
            var result = string.Join(Environment.NewLine, cardsForExport);

            return result;
        }

        public async Task Import(string deckText, Guid deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cardTextComments = converter.GetCardsFromDeck(deckText).ToList();

            foreach (var cardTextComment in cardTextComments)
            {
                var clozeNames = converter.GetClozeNames(cardTextComment.Item1);
                var isValid = clozeNames.Any() && clozeNames.All(clozeName => validator.Validate(cardTextComment.Item1, clozeName));
                var newCard = new Card(deck, cardTextComment.Item1, cardTextComment.Item2, isValid);

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

        public async Task<string> Backup()
        {
            var backup = new BackupModel
            {
                Decks = await repository.GetAllDecksAsync(),
                Answers = await repository.GetAllAnswersAsync()
            };

            var result = JsonConvert.SerializeObject(backup, Formatting.Indented);
            return result;
        }

        public async Task Restore(Stream inputStream)
        {
            BackupModel GetBackup(Stream stream)
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var jsonSerializer = new JsonSerializer();

                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var backupModel = jsonSerializer.Deserialize<BackupModel>(jsonReader);
                        return backupModel;
                    }
                }
            }

            var backup = GetBackup(inputStream);

            repository.RemoveDecks();
            repository.RemoveAnswers();

            foreach (var deck in backup.Decks)
            {
                repository.AddDeck(deck);
            }

            foreach (var answer in backup.Answers)
            {
                repository.AddAnswer(answer);
            }

            await repository.SaveChangesAsync();
        }
    }
}

