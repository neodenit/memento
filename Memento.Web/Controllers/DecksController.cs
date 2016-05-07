using Memento.Attributes;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Memento.Web.Controllers
{
    [Authorize]
#if !DEBUG
    [RequireHttps]
#endif
    public class DecksController : Controller
    {
        private readonly IDecksService decksService;
        private readonly ICardsService cardsService;
        private readonly IStatisticsService statService;
        private readonly IExportImportService exportImportService;
        private readonly ISchedulerService schedulerService;
        private readonly IFactory factory;

        public DecksController(IDecksService decksService, ICardsService cardsService, IStatisticsService statService, IExportImportService exportImportService, ISchedulerService schedulerService, IFactory factory)
        {
            this.decksService = decksService;
            this.cardsService = cardsService;
            this.statService = statService;
            this.exportImportService = exportImportService;
            this.schedulerService = schedulerService;
            this.factory = factory;
        }

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var decks = await decksService.GetDecksAsync(User.Identity.Name);
            return View(decks.Cast<Deck>());
        }

        // GET: Decks/Details/5
        public async Task<ActionResult> Details([CheckDeckExistence, CheckDeckOwner] int id)
        {
            var startTime = DateTime.Now.AddDays(-10);

            var answers = await statService.GetAnswersAsync(id, startTime);

            var statistics = statService.GetStatistics(answers);

            var viewModel = await decksService.GetDeckWithStatViewModel(id, statistics);

            return View(viewModel);
        }

        // POST: Decks/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details([Bind(Include = "ID")] Deck deck)
        {
            var dbDeck = await decksService.FindDeckAsync(deck.ID);
            var card = dbDeck.GetNextCard();

            if (card != null)
            {
                return RedirectToAction("Details", "Cards", new { card.ID });
            }
            else
            {
                return View("EmptyDeck", new DeckViewModel(dbDeck));
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
        public async Task<ActionResult> Create([Bind(Include = "Title, ControlMode, DelayMode, StartDelay, Coeff, FirstDelay, SecondDelay")] DeckViewModel deck)//todo name
        {
            if (ModelState.IsValid)
            {
                var newDeck = new Deck
                {
                    AllowSmallDelays = Settings.Default.AllowSmallDelays,
                    Title = deck.Title,
                    DelayMode = Settings.Default.AllowSmoothDelayModes ? deck.DelayMode : DelayModes.Sharp,
                    ControlMode = deck.ControlMode,
                    StartDelay = Settings.Default.EnableTwoStepsConfig ? deck.FirstDelay : deck.StartDelay,
                    Coeff = Settings.Default.EnableTwoStepsConfig ?
                        (double)deck.SecondDelay / deck.FirstDelay :
                        deck.Coeff,
                };

                await decksService.CreateDeck(newDeck, User.Identity.Name);

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        // GET: Decks/Edit/5
        public async Task<ActionResult> Edit([CheckDeckExistence, CheckDeckOwner] int id)
        {
            var deck = await decksService.FindDeckAsync(id);
            var viewModel = new DeckViewModel(deck);

            return View(viewModel);
        }

        // POST: Decks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID, Title, StartDelay, Coeff, FirstDelay, SecondDelay")] DeckViewModel deck)
        {
            if (ModelState.IsValid)
            {
                var delay = Settings.Default.EnableTwoStepsConfig ? deck.FirstDelay : deck.StartDelay;
                var coeff = Settings.Default.EnableTwoStepsConfig ?
                        (double)deck.SecondDelay / deck.FirstDelay :
                        deck.Coeff;

                await decksService.UpdateDeck(deck.ID, deck.Title, delay, coeff);

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        public async Task<ActionResult> Delete([CheckDeckExistence, CheckDeckOwner] int id)
        {
            var deck = await decksService.FindDeckAsync(id);

            return View(deck);
        }

        // POST: Decks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CheckDeckExistence, CheckDeckOwner] int id)
        {
            await decksService.DeleteDeck(id);

            return RedirectToAction("Index");
        }

        public ActionResult Import([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var viewModel = new ImportViewModel { DeckID = deckID, IsShuffled = true };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Import(ImportViewModel viewModel, HttpPostedFileBase file)
        {
            if (file?.ContentLength > 0)
            {
                var text = await new StreamReader(file.InputStream).ReadToEndAsync();

                await exportImportService.Import(text, viewModel.DeckID);

                if (viewModel.IsShuffled)
                {
                    await schedulerService.ShuffleNewClozes(viewModel.DeckID);
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Export([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var fileContentText = await exportImportService.Export(deckID);
            var deck = await decksService.FindDeckAsync(deckID);
            var deckTitle = deck.Title;
            var fileName = string.Join(string.Empty, deckTitle.Split(Path.GetInvalidFileNameChars()));

            return File(Encoding.UTF8.GetBytes(fileContentText), MediaTypeNames.Text.Plain, $"{fileName}.txt");
        }
    }
}
