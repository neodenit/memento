using Memento.DomainModel.Attributes;
using Memento.DomainModel.Models;
using Memento.DomainModel.Repository;
using Memento.DomainModel.ViewModels;
using Memento.Interfaces;
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

        public CardsController(IMementoRepository repository, IConverter converter, IValidator validator, IScheduler scheduler, IEvaluator evaluator)
        {
            this.repository = repository;
            this.evaluator = evaluator;
            this.converter = converter;
            this.validator = validator;
            this.scheduler = scheduler;
        }

        public async Task<ActionResult> ClozesIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var clozes = deck.GetClozes();
            var orderedClozes = clozes.OrderBy(cloze => cloze.Position);
            var clozeViews = from cloze in orderedClozes select new ClozeViewModel(cloze);

            return View(clozeViews);
        }

        public async Task<ActionResult> CardsIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cards = deck.GetValidCards();

            return View(cards);
        }

        public async Task<ActionResult> DeletedIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cards = deck.GetDeletedCards();
            var viewModel = from card in cards select new EditCardViewModel(card);

            return View(viewModel);
        }

        public async Task<ActionResult> DraftIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await repository.FindDeckAsync(deckID);
            var cards = deck.GetDraftCards();
            var viewModel = from card in cards select new EditCardViewModel(card);

            return View(viewModel);
        }

        public ActionResult DetailsEmpty([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            return View("Details", new Card { DeckID = deckID, ID = -1 });
        }

        public async Task<ActionResult> Details([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            var cloze = card.GetNextCloze();

            if (cloze.IsNew)
            {
                return RedirectToAction("PreviewClosed", new { id = card.ID });
            }
            else if (card.Deck.ControlMode == ControlModes.Manual)
            {
                return RedirectToAction("RepeatClosed", new { id = card.ID });
            }
            else if (card.Deck.ControlMode == ControlModes.Automatic)
            {
                return RedirectToAction("Question", new { id = card.ID });
            }
            else
            {
                throw new Exception();
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

            return await PromoteAndRedirect(dbCard.Deck, Delays.Same);
        }

        public async Task<ActionResult> RepeatClosed([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            var cloze = card.GetNextCloze();
            var question = converter.GetQuestion(card.Text, cloze.Label);

            card.Text = question;

            return View(card);
        }

        public async Task<ActionResult> RepeatOpened([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            var cloze = card.GetNextCloze();
            var question = converter.GetAnswer(card.Text, cloze.Label);

            card.Text = question;

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RepeatOpened([Bind(Include = "ID")] Card card, string againButton, string badButton, string goodButton)
        {
            var dbCard = await repository.FindCardAsync(card.ID);

            var cloze = dbCard.GetNextCloze();

            var isCorrect = goodButton != null;

            repository.AddAnswer(cloze, isCorrect);

            await repository.SaveChangesAsync();

            var delay = againButton != null ?
                        Delays.Initial :
                        badButton != null ?
                        Delays.Previous :
                        goodButton != null ?
                        Delays.Next :
                        Delays.Same;

            return await PromoteAndRedirect(dbCard.Deck, delay);
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

            return await PromoteAndRedirect(dbCard.Deck, Delays.Next);
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

                return await PromoteAndRedirect(dbCard.Deck, Delays.Previous);
            }
            else if (AltButton != null)
            {
                dbCard.Text = converter.AddAnswer(dbCard.Text, cloze.Label, card.Answer);

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
                return await PromoteAndRedirect(dbCard.Deck, Delays.Previous);
            }
            else if (AltButton != null)
            {
                var cloze = dbCard.GetNextCloze();

                dbCard.Text = converter.AddAnswer(dbCard.Text, cloze.Label, card.Answer);

                await repository.SaveChangesAsync();

                return RedirectToAction("Details", new { id = card.ID });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID, DeckID, Text")] EditCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                var text = card.Text;
                var clozeNames = converter.GetClozeNames(text);

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
            else
            {
                return View(card);
            }
        }

        public async Task<ActionResult> Edit([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);
            var cardViewModel = new EditCardViewModel(card);

            return View(cardViewModel);
        }

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

            scheduler.ShuffleNewClozes(clozes);

            await repository.SaveChangesAsync();

            return RedirectToAction("ClozesIndex", new { deckID });
        }

        public async Task<ActionResult> Restore([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);

            return View(card);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreConfirmed([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);

            card.IsDeleted = false;

            await repository.SaveChangesAsync();

            return RedirectToAction("DeletedIndex", "Cards", new { DeckID = card.DeckID });
        }

        public async Task<ActionResult> Delete([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);

            return View(card);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await repository.FindCardAsync(id);

            if (card.IsDeleted)
            {
                repository.RemoveCard(card);
            }
            else
            {
                card.IsDeleted = true;
            }

            await repository.SaveChangesAsync();

            return RedirectToAction("Details", "Decks", new { id = card.DeckID });
        }

        private async Task<ActionResult> PromoteAndRedirect(Deck deck, Delays delay)
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
