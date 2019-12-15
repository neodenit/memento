using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neodenit.Memento.Attributes;
using Neodenit.Memento.Common;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.ViewModels;

namespace Neodenit.Memento.Web.Controllers
{
    [Authorize]
    public class DecksController : Controller
    {
        private readonly IDecksService decksService;
        private readonly ICardsService cardsService;
        private readonly IStatisticsService statService;
        private readonly IExportImportService exportImportService;
        private readonly ISchedulerService schedulerService;

        public DecksController(IDecksService decksService, ICardsService cardsService, IStatisticsService statService, IExportImportService exportImportService, ISchedulerService schedulerService)
        {
            this.decksService = decksService;
            this.cardsService = cardsService ?? throw new ArgumentNullException(nameof(cardsService));
            this.statService = statService;
            this.exportImportService = exportImportService;
            this.schedulerService = schedulerService;
        }

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var decks = await decksService.GetDecksAsync(User.Identity.Name);
            var sharedDecks = await decksService.GetSharedDecksAsync();

            var viewModel = new DecksViewModel
            {
                UserDecks = decks,
                SharedDecks = sharedDecks,
            };

            return View(viewModel);
        }

        public async Task<ActionResult> Share([CheckDeckExistence, CheckDeckOwner] Guid id)
        {
            await decksService.ShareDeckAsync(id);

            return RedirectToAction("Index");
        }

        // GET: Decks/Details/5
        public async Task<ActionResult> Details([CheckDeckExistence, CheckDeckOwner] Guid id)
        {
            var startTime = DateTime.Now.AddDays(-10);

            var answers = await statService.GetAnswersAsync(id, startTime);

            var statistics = statService.GetStatistics(answers);

            var viewModel = await decksService.GetDeckWithStatViewModel(id, statistics, User.Identity.Name);

            return View(viewModel);
        }

        // POST: Decks/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details(DeckViewModel deck)
        {
            var card = await cardsService.GetNextCardAsync(deck.ID, User.Identity.Name);

            if (card != null)
            {
                return RedirectToAction("Details", "Cards", new { card.ID });
            }
            else
            {
                return View("EmptyDeck", deck);
            }
        }

        // GET: Decks/Create
        public ActionResult Create()
        {
            var viewModel = Settings.Default.EnableTwoStepsConfig ?
                new DeckViewModel { FirstDelay = Settings.Default.FirstDelay, SecondDelay = Settings.Default.SecondDelay } :
                new DeckViewModel { StartDelay = Settings.Default.StartDelay, Coeff = Settings.Default.Coeff };

            return View(viewModel);
        }

        // POST: Decks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DeckViewModel deck)
        {
            if (ModelState.IsValid)
            {
                await decksService.CreateDeck(deck, User.Identity.Name);

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        // GET: Decks/Edit/5
        public async Task<ActionResult> Edit([CheckDeckExistence, CheckDeckOwner] Guid id)
        {
            var deck = await decksService.FindDeckAsync(id);

            return View(deck);
        }

        // POST: Decks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DeckViewModel deck)
        {
            if (ModelState.IsValid)
            {
                var delay = Settings.Default.EnableTwoStepsConfig ?
                    deck.FirstDelay :
                    deck.StartDelay;
                var coeff = Settings.Default.EnableTwoStepsConfig ?
                    (double)deck.SecondDelay / (double)deck.FirstDelay :
                    deck.Coeff;

                await decksService.UpdateDeck(deck.ID, deck.Title, delay, coeff, deck.PreviewAnswer);

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        public async Task<ActionResult> Delete([CheckDeckExistence, CheckDeckOwner] Guid id)
        {
            var deck = await decksService.FindDeckAsync(id);

            return View(deck);
        }

        // POST: Decks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CheckDeckExistence, CheckDeckOwner] Guid id)
        {
            await decksService.DeleteDeck(id);

            return RedirectToAction("Index");
        }

        public ActionResult Import([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            var viewModel = new ImportViewModel { DeckID = deckID, IsShuffled = true };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Import(ImportViewModel viewModel, IFormFile file)
        {
            using (var streamReader = new StreamReader(file.OpenReadStream()))
            {
                var deckText = await streamReader.ReadToEndAsync();

                await exportImportService.Import(deckText, viewModel.DeckID);
            }

            if (viewModel.IsShuffled)
            {
                await schedulerService.ShuffleNewClozes(viewModel.DeckID, User.Identity.Name);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Export([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            var fileContentText = await exportImportService.Export(deckID);
            var deck = await decksService.FindDeckAsync(deckID);
            var deckTitle = deck.Title;
            var fileName = string.Join(string.Empty, deckTitle.Split(Path.GetInvalidFileNameChars()));

            return File(Encoding.UTF8.GetBytes(fileContentText), MediaTypeNames.Text.Plain, $"{fileName}.txt");
        }

        public ActionResult Backup()
        {
            return View();
        }

        public async Task<ActionResult> GetBackupFile()
        {
            string fileContentText = await exportImportService.Backup();

            return File(Encoding.UTF8.GetBytes(fileContentText), MediaTypeNames.Text.Plain, $"MEMENTO-BACKUP-{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.json");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Restore(IFormFile file)
        {
            await exportImportService.Restore(file.OpenReadStream());

            return View("Backup");
        }
    }
}
