using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ionic.Zip;
using Memento.Core;
using Memento.DomainModel;
using Memento.DomainModel.Repository;

namespace Memento.Web.Controllers
{
    [Authorize]
#if !DEBUG
    [RequireHttps]
#endif
    public class DecksController : Controller
    {
        private readonly IMementoRepository repository;

        public DecksController()
        {
            repository = new EFMementoRepository();
        }

        public DecksController(IMementoRepository repository)
        {
            this.repository = repository;
        }

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var decks = repository.GetUserDecks(User.Identity.Name);

            var orderedDecks = decks.OrderBy(deck => deck.Title);

            return View(await orderedDecks.ToListAsync());
        }

        public ActionResult Export()
        {
            var decks = repository.GetUserDecks(User.Identity.Name);

            return new JsonResult { Data = decks, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // GET: Decks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await repository.FindDeckAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
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
        }

        // POST: Decks/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details([Bind(Include = "ID")]Deck deck)
        {
            var dbDeck = await repository.FindDeckAsync(deck.ID);

            if (dbDeck == null)
            {
                return HttpNotFound();
            }
            else if (!dbDeck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else if (dbDeck.Cards.Any())
            {
                var card = dbDeck.GetNextCard();

                return RedirectToAction("Details", "Cards", new { id = card.ID });
            }
            else
            {
                return RedirectToAction("DetailsEmpty", "Cards", new { DeckID = dbDeck.ID });
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
        public async Task<ActionResult> Create([Bind(Include = "Title,ControlMode,DelayMode,StartDelay,Coeff")] Deck deck)
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
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await repository.FindDeckAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(deck);
            }
        }

        // POST: Decks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Title,StartDelay,Coeff")] Deck deck)
        {
            if (ModelState.IsValid)
            {
                var dbDeck = await repository.FindDeckAsync(deck.ID);

                if (deck == null)
                {
                    return HttpNotFound();
                }
                else if (!dbDeck.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }

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

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await repository.FindDeckAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(deck);
            }
        }

        // POST: Decks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var deck = await repository.FindDeckAsync(id);

            if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            repository.RemoveDeck(deck);

            await repository.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public ActionResult Import(int DeckID)
        {
            var deckWithID = new Deck { ID = DeckID };

            return View(deckWithID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Import(Deck deckWithID, HttpPostedFileBase file)
        {
            var deck = await repository.FindDeckAsync(deckWithID.ID);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            if (file != null && file.ContentLength > 0)
            {
                var text = await new StreamReader(file.InputStream).ReadToEndAsync();

                var cards = Converter.GetCardsFromDeck(text, true);

                foreach (var card in cards)
                {
                    var cardText = HttpUtility.HtmlDecode(card);

                    var clozeNames = Converter.GetClozeNames(cardText);

                    var updatedText = Converter.ReplaceAllWithWildCards(cardText, clozeNames);

                    if (!clozeNames.All(clozeName => Validator.ValidateFull(cardText, clozeName)))
                    {
                        var newCard = new Card
                        {
                            DeckID = deckWithID.ID,
                            Text = updatedText,
                            IsValid = false,
                            Answer = updatedText,
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
                            Answer = updatedText,
                        };

                        repository.AddCard(newCard);

                        await repository.SaveChangesAsync();

                        foreach (var clozeName in clozeNames)
                        {
                            var newCloze = new Cloze(newCard.ID, clozeName);

                            var deckClozes = deck.GetClozes();

                            Scheduler.PrepareForAdding(deck, deckClozes, newCloze);

                            repository.AddCloze(newCloze);
                        }
                    }

                    await repository.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
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
