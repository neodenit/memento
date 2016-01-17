using Ionic.Zip;
using Memento.Core;
using Memento.Core.Validators;
using Memento.DomainModel.Attributes;
using Memento.DomainModel.Models;
using Memento.DomainModel.Repository;
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
        private readonly IMementoRepository repository;
        private readonly IConverter converter;
        private readonly IValidator validator;
        private readonly IScheduler scheduler;

        public DecksController(IMementoRepository repository, IConverter converter, IValidator validator, IScheduler scheduler)
        {
            this.repository = repository;
            this.converter = converter;
            this.validator = validator;
            this.scheduler = scheduler;
        }

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var decks = repository.GetUserDecks(User.Identity.Name);
            var orderedDecks = decks.OrderBy(deck => deck.Title);

            return View(await orderedDecks.ToListAsync());
        }

        // GET: Decks/Details/5
        public async Task<ActionResult> Details([CheckDeckExistence, CheckDeckOwner] int id)
        {
            var deck = await repository.FindDeckAsync(id);
            var startTime = DateTime.Now.AddDays(-10);
            var answers = repository
                .GetAnswersForDeck(deck.ID)
                .Where(answer => answer.Time >= startTime)
                .ToList();

            var groupedAnswers = from answer in answers group answer by answer.Time.Date;

            var answerLabels = from item in groupedAnswers select item.Key.ToShortDateString();
            var answerValues = from item in groupedAnswers select item.Count();

            var groupedCorrectAnswers = from answer in answers where answer.IsCorrect group answer by answer.Time.Date;

            var correctAnswerLabels = from item in groupedCorrectAnswers select item.Key.ToShortDateString();
            var correctAnswerValues = from item in groupedCorrectAnswers select item.Count();

            var cardsLabels = answerLabels;
            var cardsValues = from item in groupedAnswers select item.GetMaxElement(x => x.Time).CardsInRepetition;

            ViewBag.Answers = new { labels = answerLabels, values = answerValues };
            ViewBag.CorrectAnswers = new { labels = correctAnswerLabels, values = correctAnswerValues };
            ViewBag.Cards = new { labels = cardsLabels, values = cardsValues };

            return View(deck);
        }

        // POST: Decks/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details([Bind(Include = "ID")] Deck deck)
        {
            var dbDeck = await repository.FindDeckAsync(deck.ID);

            if (dbDeck.GetValidCards().Any())
            {
                var card = dbDeck.GetNextCard();

                return RedirectToAction("Details", "Cards", new { id = card.ID });
            }
            else
            {
                return RedirectToAction("DetailsEmpty", "Cards", new { deckID = dbDeck.ID });
            }
        }

        // GET: Decks/Create
        public ActionResult Create()
        {
            var deck = new Deck { Coeff = Settings.Default.Coeff, StartDelay = Settings.Default.StartDelay };

            return View(deck);
        }

        // POST: Decks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Title, ControlMode, DelayMode, StartDelay, Coeff")] Deck deck)
        {
            if (deck.ControlMode == ControlModes.Automatic && deck.DelayMode == DelayModes.Combined)
            {
                throw new Exception();
            }
            else if (ModelState.IsValid)
            {

                deck.Owner = User.Identity.Name;

                repository.AddDeck(deck);

                await repository.SaveChangesAsync();

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
            var deck = await repository.FindDeckAsync(id);

            return View(deck);
        }

        // POST: Decks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID, Title, StartDelay, Coeff")] Deck deck)
        {
            if (ModelState.IsValid)
            {
                var dbDeck = await repository.FindDeckAsync(deck.ID);

                dbDeck.Title = deck.Title;
                dbDeck.StartDelay = deck.StartDelay;
                dbDeck.Coeff = deck.Coeff;

                await repository.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        public async Task<ActionResult> Delete([CheckDeckExistence, CheckDeckOwner] int id)
        {
            var deck = await repository.FindDeckAsync(id);

            return View(deck);
        }

        // POST: Decks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CheckDeckExistence, CheckDeckOwner] int id)
        {
            var deck = await repository.FindDeckAsync(id);
            
            repository.RemoveDeck(deck);

            await repository.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public ActionResult Import([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deckWithID = new Deck { ID = deckID };

            return View(deckWithID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Import(Deck deckWithID, HttpPostedFileBase file)
        {
            var deck = await repository.FindDeckAsync(deckWithID.ID);
            
            if (file != null && file.ContentLength > 0)
            {
                var text = await new StreamReader(file.InputStream).ReadToEndAsync();
                var cards = converter.GetCardsFromDeck(text);

                foreach (var card in cards)
                {
                    var cardText = HttpUtility.HtmlDecode(card);
                    var clozeNames = converter.GetClozeNames(cardText);
                    var updatedText = converter.ReplaceTextWithWildcards(cardText, clozeNames);
                    var isValid = clozeNames.Any() && clozeNames.All(clozeName => validator.Validate(cardText, clozeName));

                    if (!isValid)
                    {
                        var newCard = new Card
                        {
                            DeckID = deckWithID.ID,
                            Text = cardText,
                            IsValid = false,
                        };

                        repository.AddCard(newCard);
                    }
                    else
                    {
                        var newCard = new Card
                        {
                            DeckID = deckWithID.ID,
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

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Export([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);            
            var cards = deck.GetAllCards();
            var cardsForExport = from card in cards select converter.FormatForExport(card.Text);
            var fileContentText = string.Join(Environment.NewLine, cardsForExport);

            return File(Encoding.UTF8.GetBytes(fileContentText), MediaTypeNames.Text.Plain, "Export.txt");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repository.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
