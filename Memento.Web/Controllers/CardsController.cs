using Memento.Core;
using Memento.Core.Evaluators;
using Memento.Core.Validators;
using Memento.DomainModel.Attributes;
using Memento.DomainModel.Models;
using Memento.DomainModel.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Memento.Web.Controllers
{
    [Authorize]
#if !DEBUG
    [RequireHttps]
#endif
    public class CardsController : Controller
    {
        private readonly IMementoRepository repository;
        private readonly IEvaluator evaluator;
        private readonly IConverter converter;
        private readonly IValidator validator;
        private readonly IScheduler scheduler;

        public CardsController(IMementoRepository repository, IEvaluator evaluator, IConverter converter, IValidator validator, IScheduler scheduler)
        {
            this.repository = repository;
            this.evaluator = evaluator;
            this.converter = converter;
            this.validator = validator;
            this.scheduler = scheduler;
        }

        // GET: Cards
        public async Task<ActionResult> Index([CheckDeckExistence, CheckDeckOwner] int DeckID)
        {
            var deck = await repository.FindDeckAsync(DeckID);
            var clozes = deck.GetClozes();
            var orderedClozes = clozes.OrderBy(cloze => cloze.Position);
            var clozeViews = from cloze in orderedClozes select new ClozeView(cloze);

            return View(clozeViews);
        }

        public async Task<ActionResult> CardsIndex([CheckDeckExistence, CheckDeckOwner] int DeckID)
        {
            var deck = await repository.FindDeckAsync(DeckID);
            var cards = deck.GetValidCards();

            return View(cards);
        }

        public ActionResult DetailsEmpty([CheckDeckExistence, CheckDeckOwner] int DeckID)
        {
            return View("Details", new Card { DeckID = DeckID, ID = -1 });
        }

        public async Task<ActionResult> Details([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
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

        public async Task<ActionResult> PreviewClosed([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            var cloze = card.GetNextCloze();
            var question = converter.GetQuestion(card.Text, cloze.Label);

            card.Text = question;

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewClozed([Bind(Include = "ID")] Card card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            return RedirectToAction("PreviewOpened", new { id = dbCard.ID });
        }

        public async Task<ActionResult> PreviewOpened([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            var cloze = card.GetNextCloze();
            var question = converter.GetAnswer(card.Text, cloze.Label);

            card.Text = question;

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewOpened([Bind(Include = "ID")] Card card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            return await PromoteAndRedirect(dbCard.Deck, Scheduler.Delays.Same);
        }

        public async Task<ActionResult> Question([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            var cloze = card.GetNextCloze();
            var cardViewModel = new AnswerCardViewModel(card);

            cardViewModel.Text = converter.GetQuestion(card.Text, cloze.Label);

            return View(cardViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Question([Bind(Include = "ID, Answer")] AnswerCardViewModel card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);
            var cloze = dbCard.GetNextCloze();
            var cardViewModel = new AnswerCardViewModel(dbCard);
            var answer = converter.GetAnswerValue(dbCard.Text, cloze.Label);
            var result = evaluator.Evaluate(answer, card.Answer);

            switch (result)
            {
                case Mark.Correct:
                    cardViewModel.Text = converter.GetAnswer(dbCard.Text, cloze.Label);

                    return View("Right", cardViewModel);
                case Mark.Incorrect:
                    cardViewModel.Text = converter.GetAnswer(dbCard.Text, cloze.Label);

                    return View("Wrong", cardViewModel);
                case Mark.Typo:
                    cardViewModel.Text = converter.GetAnswer(dbCard.Text, cloze.Label);

                    return View("Typo", cardViewModel);
                default:
                    throw new Exception();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Right([Bind(Include = "ID")] AnswerCardViewModel card)
        {
            var dbCard = await repository.FindCardAsync(card.ID);
            var cloze = dbCard.GetNextCloze();

            repository.AddAnswer(cloze, true);

            await repository.SaveChangesAsync();

            return await PromoteAndRedirect(dbCard.Deck, Scheduler.Delays.Next);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wrong([Bind(Include = "ID, Answer")] AnswerCardViewModel card, string NextButton, string AltButton)
        {
            var dbCard = await repository.FindCardAsync(card.ID);
            var cloze = dbCard.GetNextCloze();

            if (NextButton != null)
            {
                repository.AddAnswer(cloze, false);

                await repository.SaveChangesAsync();

                return await PromoteAndRedirect(dbCard.Deck, Scheduler.Delays.Previous);
            }
            else if (AltButton != null)
            {
                var oldAnswers = converter.GetAnswerValue(dbCard.Text, cloze.Label);
                var newAnswers = string.Format("{0}|{1}", oldAnswers, card.Answer);
                var newText = converter.ReplaceAnswer(dbCard.Text, cloze.Label, newAnswers);

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
        public async Task<ActionResult> Typo([Bind(Include = "ID, Answer")] AnswerCardViewModel card, string TypoButton, string WrongButton, string AltButton)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

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
                var oldAnswers = converter.GetAnswerValue(dbCard.Text, cloze.Label);
                var newAnswers = string.Format("{0}|{1}", oldAnswers, card.Answer);
                var newText = converter.ReplaceAnswer(dbCard.Text, cloze.Label, newAnswers);

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

                var card = new EditCardViewModel { DeckID = -1 };

                return View(card);
            }
            else
            {
                var card = new EditCardViewModel { DeckID = DeckID.Value };

                return View(card);
            }
        }

        // POST: Cards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID, DeckID, Text")] EditCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                var text = card.Text;
                var clozeNames = converter.GetClozeNames(text);

                if (!clozeNames.Any() || !clozeNames.All(clozeName => validator.Validate(text, clozeName)))
                {
                    return View(card);
                }
                else
                {
                    var newCard = new Card
                    {
                        ID = card.ID,
                        DeckID = card.DeckID,
                        Deck = await repository.FindDeckAsync(card.DeckID),
                        Text = converter.ReplaceTextWithWildcards(card.Text, clozeNames),
                        Clozes = new Collection<Cloze>(),
                        IsValid = true,
                        IsDeleted = false,
                    };

                    repository.AddCard(newCard);

                    await repository.SaveChangesAsync();

                    repository.AddClozes(newCard, clozeNames);

                    await repository.SaveChangesAsync();

                    return RedirectToAction("Create", "Cards", new { DeckID = card.DeckID });
                }
            }
            else
            {
                return View(card);
            }
        }

        // GET: Cards/Edit/5
        public async Task<ActionResult> Edit([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            var cardViewModel = new EditCardViewModel(card);

            return View(cardViewModel);
        }

        // POST: Cards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID, Text, Answer")] Card card)
        {
            if (ModelState.IsValid)
            {
                var dbCard = await repository.FindCardAsync(card.ID);
                var clozes = converter.GetClozeNames(dbCard.Text);

                dbCard.Text = converter.ReplaceTextWithWildcards(card.Text, clozes);

                var oldClozes = from cloze in dbCard.Clozes select cloze.Label;
                var newClozes = clozes;

                var deletedClozes = oldClozes.Except(newClozes).ToList();
                var addedClozes = newClozes.Except(oldClozes).ToList();

                repository.RemoveClozes(dbCard, deletedClozes);
                repository.AddClozes(dbCard, addedClozes);

                await repository.SaveChangesAsync();

                return RedirectToAction("Details", "Decks", new { id = dbCard.DeckID });
            }
            else
            {
                return View(card);
            }
        }

        public async Task<ActionResult> ShuffleNewCards(int cardID)
        {
            var card = await repository.FindCardAsync(cardID);

            return await ShuffleNew(card.DeckID);
        }

        public async Task<ActionResult> ShuffleNew([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var clozes = deck.GetClozes();

            scheduler.ShuffleNewCards(clozes);

            await repository.SaveChangesAsync();

            return RedirectToAction("Index", new { deckID });
        }

        // GET: Cards/Delete/5
        public async Task<ActionResult> Delete([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);

            return View(card);
        }

        // POST: Cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            
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
