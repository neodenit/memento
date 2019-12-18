using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.Services.API;
using Newtonsoft.Json;

namespace Neodenit.Memento.Services
{
    public class ExportImportService : IExportImportService
    {
        private readonly IMementoRepository repository;
        private readonly IConverterService converterService;
        private readonly IValidatorService validatorService;
        private readonly IClozesService clozesService;

        public ExportImportService(IMementoRepository repository, IConverterService converterService, IValidatorService validatorService, IClozesService clozesService)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.converterService = converterService ?? throw new ArgumentNullException(nameof(converterService));
            this.validatorService = validatorService ?? throw new ArgumentNullException(nameof(validatorService));
            this.clozesService = clozesService ?? throw new ArgumentNullException(nameof(clozesService));
        }

        public async Task<string> Export(Guid deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cards = deck.GetValidCards();
            var cardsForExport = from card in cards select converterService.FormatForExport(card.Text, card.Comment);
            var result = string.Join(Environment.NewLine, cardsForExport);

            return result;
        }

        public async Task Import(string deckText, Guid deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cardTextComments = converterService.GetCardsFromDeck(deckText).ToList();

            foreach (var cardTextComment in cardTextComments)
            {
                var clozeNames = converterService.GetClozeNames(cardTextComment.Item1);
                var isValid = clozeNames.Any() && clozeNames.All(clozeName => validatorService.Validate(cardTextComment.Item1, clozeName));
                var newCard = new Card
                {
                    Deck = deck,
                    Text = cardTextComment.Item1,
                    Comment = cardTextComment.Item2,
                    IsValid = true,
                    ID = Guid.NewGuid()
                };

                if (!string.IsNullOrWhiteSpace(newCard.Text))
                {
                    deck.Cards.Add(newCard);

                    if (isValid)
                    {
                        clozesService.AddClozes(newCard, clozeNames);
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

