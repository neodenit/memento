using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Memento.Core;
using Memento.DomainModel;
using Memento.DomainModel.Repository;
using Memento.Core.Evaluators;

namespace Memento.Web.Controllers
{
    [Authorize]
#if !DEBUG
    [RequireHttps]
#endif
    public class CardsController : Controller
    {
        private readonly IMementoRepository repository;

        public CardsController()
        {
            repository = new EFMementoRepository();
        }

        public CardsController(IMementoRepository repository)
        {
            this.repository = repository;
        }

        // GET: Cards
        public async Task<ActionResult> Index(int? DeckID)
        {
            if (DeckID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var deck = await repository.FindDeckAsync(DeckID);

                if (!deck.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    var clozes = deck.GetClozes();

                    var orderedClozes = clozes.OrderBy(cloze => cloze.Position);

                    return View(orderedClozes);
                }
            }
        }

        public ActionResult DetailsEmpty(int? DeckID)
        {
            if (DeckID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                return View("Details", new Card { DeckID = DeckID.Value, ID = -1 });
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            var card = await repository.FindCardAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            var cloze = card.GetNextCloze();

            if (cloze.IsNew)
            {
                return RedirectToAction("PreviewClosed", new { id = card.ID });
            }
            else
            {
                return RedirectToAction("Question", new { id = card.ID });
            }
        }

        public async Task<ActionResult> PreviewClosed(int id)
        {
            var card = await repository.FindCardAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            var cloze = card.GetNextCloze();

            var question = Converter.GetQuestion(card.Text, cloze.Label);

            card.Text = question;

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewClozed([Bind(Include = "ID")]Card card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            if (dbCard == null)
            {
                return HttpNotFound();
            }
            else if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            return RedirectToAction("PreviewOpened", new { id = dbCard.ID });
        }

        public async Task<ActionResult> PreviewOpened(int id)
        {
            var card = await repository.FindCardAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            var cloze = card.GetNextCloze();

            var question = Converter.GetAnswer(card.Text, cloze.Label);

            card.Text = question;

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewOpened([Bind(Include = "ID")]Card card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            if (dbCard == null)
            {
                return HttpNotFound();
            }
            else if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return await PromoteAndRedirect(dbCard.Deck, Scheduler.Delays.Same);
            }
        }

        public async Task<ActionResult> Question(int? id)
        {
            var card = await repository.FindCardAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var cloze = card.GetNextCloze();

                card.Text = Converter.GetQuestion(card.Text, cloze.Label);
                card.Answer = string.Empty;

                return View(card);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Question([Bind(Include = "ID, Answer")]Card card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var cloze = dbCard.GetNextCloze();

                var answer = Converter.GetAnswerValue(dbCard.Text, cloze.Label);

                var result = new PhraseEvaluator(0.2).Evaluate(answer, card.Answer);

                switch (result)
                {
                    case Mark.Correct:
                        dbCard.Text = Converter.GetAnswer(dbCard.Text, cloze.Label);

                        return View("Right", dbCard);
                    case Mark.Incorrect:
                        ViewBag.Answer = card.Answer;

                        dbCard.Text = Converter.GetAnswer(dbCard.Text, cloze.Label);

                        return View("Wrong", dbCard);
                    case Mark.Typo:
                        ViewBag.Answer = card.Answer;

                        dbCard.Text = Converter.GetAnswer(dbCard.Text, cloze.Label);

                        return View("Typo", dbCard);
                    default:
                        throw new Exception();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Right([Bind(Include = "ID")]Card card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var cloze = dbCard.GetNextCloze();

                repository.AddAnswer(cloze, true);

                await repository.SaveChangesAsync();

                return await PromoteAndRedirect(dbCard.Deck, Scheduler.Delays.Next);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wrong([Bind(Include = "ID, Answer")]Card card, string NextButton, string AltButton)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            if (NextButton != null)
            {
                var cloze = dbCard.GetNextCloze();

                repository.AddAnswer(cloze, false);

                await repository.SaveChangesAsync();

                return await PromoteAndRedirect(dbCard.Deck, Scheduler.Delays.Previous);
            }
            else if (AltButton != null)
            {
                var cloze = dbCard.GetNextCloze();

                var oldAnswers = Converter.GetAnswerValue(dbCard.Text, cloze.Label);

                var newAnswers = string.Format("{0}|{1}", oldAnswers, card.Answer);

                var newText = Converter.ReplaceAnswer(dbCard.Text, cloze.Label, newAnswers);

                dbCard.Text = newText;

                await repository.SaveChangesAsync();

                return RedirectToAction("Details", new { id = card.ID });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Typo([Bind(Include = "ID, Answer")]Card card, string TypoButton, string WrongButton, string AltButton)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            if (TypoButton != null)
            {
                return RedirectToAction("Details", new { id = card.ID });
            }
            else if (WrongButton != null)
            {
                return await PromoteAndRedirect(dbCard.Deck, Scheduler.Delays.Previous);
            }
            else if (AltButton != null)
            {
                var cloze = dbCard.GetNextCloze();

                var oldAnswers = Converter.GetAnswerValue(dbCard.Text, cloze.Label);

                var newAnswers = string.Format("{0}|{1}", oldAnswers, card.Answer);

                var newText = Converter.ReplaceAnswer(dbCard.Text, cloze.Label, newAnswers);

                dbCard.Text = newText;

                await repository.SaveChangesAsync();

                return RedirectToAction("Details", new { id = card.ID });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: Cards/Create
        public ActionResult Create(int? DeckID)
        {
            if (DeckID == null)
            {
                ViewBag.DeckID = new SelectList(repository.GetUserDecks(User.Identity.Name), "ID", "Title");

                var card = new Card { DeckID = -1 };

                return View(card);
            }
            else
            {
                var card = new Card { DeckID = DeckID.Value };

                return View(card);
            }
        }

        // POST: Cards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,DeckID,Text")] Card card)
        {
            if (ModelState.IsValid)
            {
                var deck = await repository.FindDeckAsync(card.DeckID);

                var text = card.Text;

                var clozeNames = Converter.GetClozeNames(text);

                if (!clozeNames.Any() || !clozeNames.All(clozeName => Validator.ValidateBase(text, clozeName)))
                {
                    return View(card);
                }
                else
                {
                    card.Text = Converter.ReplaceTextWithWildcards(card.Text, clozeNames);

                    repository.AddCard(card);

                    await repository.SaveChangesAsync();

                    repository.AddClozes(card, clozeNames);

                    await repository.SaveChangesAsync();

                    return RedirectToAction("Details", "Deck", new { id = card.DeckID });
                }
            }
            else
            {
                return View(card);
            }
        }

        // GET: Cards/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var card = await repository.FindCardAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(card);
            }
        }

        // POST: Cards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Text,Answer")] Card card)
        {
            if (ModelState.IsValid)
            {
                var dbCard = await repository.FindCardAsync(card.ID);

                if (!dbCard.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    var clozes = Converter.GetClozeNames(dbCard.Text);

                    dbCard.Text = Converter.ReplaceTextWithWildcards(card.Text, clozes);

                    var oldClozes = from cloze in dbCard.Clozes select cloze.Label;
                    var newClozes = clozes;

                    var deletedClozes = oldClozes.Except(newClozes).ToList();
                    var addedClozes = newClozes.Except(oldClozes).ToList();

                    repository.RemoveClozes(dbCard, deletedClozes);
                    repository.AddClozes(dbCard, addedClozes);

                    await repository.SaveChangesAsync();

                    return RedirectToAction("Details", "Decks", new { id = dbCard.DeckID });
                }
            }
            else
            {
                return View(card);
            }
        }

        public async Task<ActionResult> ShuffleNew(int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            var clozes = deck.GetClozes();

            Scheduler.ShuffleNewCards(clozes);

            await repository.SaveChangesAsync();

            return RedirectToAction("Index", new { deckID });
        }

        // GET: Cards/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var card = await repository.FindCardAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(card);
            }
        }

        // POST: Cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var card = await repository.FindCardAsync(id);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            repository.RemoveCard(card);

            await repository.SaveChangesAsync();

            return RedirectToAction("Details", "Deck", new { id = card.DeckID });
        }

        private async Task<ActionResult> PromoteAndRedirect(Deck deck, Scheduler.Delays delay)
        {
            repository.PromoteCard(deck, delay);

            await repository.SaveChangesAsync();

            var nextCard = deck.GetNextCard();

            return RedirectToAction("Details", new { id = nextCard.ID });
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
