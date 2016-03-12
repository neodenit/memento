using Memento.Attributes;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Models.ViewModels;
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
        private readonly IDecksService decksService;
        private readonly ICardsService cardsService;
        private readonly IStatisticsService statService;
        private readonly ISchedulerService schedulerService;

        public CardsController(IDecksService decksService, ICardsService cardsService, IStatisticsService statService, ISchedulerService schedulerService)
        {
            this.decksService = decksService;
            this.cardsService = cardsService;
            this.statService = statService;
            this.schedulerService = schedulerService;
        }

        public async Task<ActionResult> ClozesIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var clozes = deck.GetClozes();
            var orderedClozes = clozes.OrderBy(cloze => cloze.Position);
            var clozeViews = from cloze in orderedClozes select new ClozeViewModel(cloze);

            return View(clozeViews);
        }

        public async Task<ActionResult> CardsIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var cards = deck.GetValidCards();

            var viewModel = from card in cards select new ViewCardViewModel(card);

            return View(viewModel);
        }

        public async Task<ActionResult> DeletedIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var cards = deck.GetDeletedCards();
            var viewModel = from card in cards select new ViewCardViewModel(card);

            return View(viewModel);
        }

        public async Task<ActionResult> DraftIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var cards = deck.GetDraftCards();
            var viewModel = from card in cards select new ViewCardViewModel(card);

            return View(viewModel);
        }

        public async Task<ActionResult> Details([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze();

            if (cloze.IsNew)
            {
                return RedirectToAction("PreviewClosed", new { id = card.ID });
            }
            else if (card.GetDeck().ControlMode == ControlModes.Manual)
            {
                return RedirectToAction("RepeatClosed", new { id = card.ID });
            }
            else if (card.GetDeck().ControlMode == ControlModes.Automatic)
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
            var card = await cardsService.GetCardWithQuestion(id);

            return View(card);
        }

        public async Task<ActionResult> PreviewOpened([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.GetCardWithAnswer(id);

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewOpened([Bind(Include = "ID")] ViewCardViewModel card)
        {
            var dbCard = await cardsService.FindCardAsync(card.ID);

            return await PromoteAndRedirect(dbCard.GetDeck(), Delays.Same);
        }

        public async Task<ActionResult> RepeatClosed([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.GetCardWithQuestion(id);

            return View(card);
        }

        public async Task<ActionResult> RepeatOpened([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.GetCardWithAnswer(id);

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RepeatOpened([Bind(Include = "ID")] ViewCardViewModel card, string againButton, string badButton, string goodButton)
        {
            var isCorrect = goodButton != null;

            await statService.AddAnswer(card.ID, isCorrect);

            var delay = againButton != null ?
                        Delays.Initial :
                        badButton != null ?
                        Delays.Previous :
                        goodButton != null ?
                        Delays.Next :
                        Delays.Same;

            var dbCard = await cardsService.FindCardAsync(card.ID);

            return await PromoteAndRedirect(dbCard.GetDeck(), delay);
        }

        public async Task<ActionResult> Question([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.GetCardWithQuestion(id);

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Question([Bind(Include = "ID, UserAnswer")] AnswerCardViewModel card)
        {
            var evaluatedCard = await cardsService.EvaluateCard(card);

            switch (evaluatedCard.Mark)
            {
                case Mark.Correct:
                    return View("Right", evaluatedCard);
                case Mark.Incorrect:
                    return View("Wrong", evaluatedCard);
                case Mark.Typo:
                    return View("Typo", evaluatedCard);
                default:
                    throw new Exception();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Right([Bind(Include = "ID")] AnswerCardViewModel card)
        {
            await statService.AddAnswer(card.ID, true);

            var dbCard = await cardsService.FindCardAsync(card.ID);

            return await PromoteAndRedirect(dbCard.GetDeck(), Delays.Next);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wrong([Bind(Include = "ID, Answer")] AnswerCardViewModel card, string NextButton, string AltButton)
        {
            if (NextButton != null)
            {
                await statService.AddAnswer(card.ID, false);

                var dbCard = await cardsService.FindCardAsync(card.ID);
                var deck = dbCard.GetDeck();
                var delay = schedulerService.GetDelayForWrongAnswer(deck.DelayMode);

                return await PromoteAndRedirect(deck, delay);
            }
            else if (AltButton != null)
            {
                await cardsService.AddAltAnswer(card.ID, card.UserAnswer);

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
            var dbCard = await cardsService.FindCardAsync(card.ID);

            if (TypoButton != null)
            {
                return RedirectToAction("Details", new { id = card.ID });
            }
            else if (WrongButton != null)
            {
                var deck = dbCard.GetDeck();
                var delay = schedulerService.GetDelayForWrongAnswer(deck.DelayMode);

                return await PromoteAndRedirect(deck, delay);
            }
            else if (AltButton != null)
            {
                await cardsService.AddAltAnswer(card.ID, card.UserAnswer);

                return RedirectToAction("Details", new { id = card.ID });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        public async Task<ActionResult> Create(int? DeckID)
        {
            if (DeckID == null)
            {
                ViewBag.DeckID = new SelectList(await decksService.GetDecksAsync(User.Identity.Name), "ID", "Title");

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
                await cardsService.AddCard(card.ID, card.DeckID, card.Text);

                return RedirectToAction("Create", "Cards", new { DeckID = card.DeckID });
            }
            else
            {
                return View(card);
            }
        }

        public async Task<ActionResult> Edit([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cardViewModel = new EditCardViewModel(card);

            return View(cardViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID, Text")] EditCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                await cardsService.UpdateCard(card.ID, card.Text);

                var dbCard = await cardsService.FindCardAsync(card.ID);

                return RedirectToAction("Details", "Decks", new { id = dbCard.DeckID });
            }
            else
            {
                return View(card);
            }
        }

        public async Task<ActionResult> ShuffleNewCards(int cardID)
        {
            var card = await cardsService.FindCardAsync(cardID);

            return await ShuffleNew(card.DeckID);
        }

        public async Task<ActionResult> ShuffleNew([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            await schedulerService.ShuffleNewClozes(deckID);

            return RedirectToAction("ClozesIndex", new { deckID });
        }

        public async Task<ActionResult> Restore([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.FindCardAsync(id);

            return View(card);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreConfirmed([CheckCardExistence, CheckCardOwner] int id)
        {
            await cardsService.RestoreCard(id);

            var card = await cardsService.FindCardAsync(id);

            return RedirectToAction("DeletedIndex", "Cards", new { DeckID = card.DeckID });
        }

        public async Task<ActionResult> Delete([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.FindCardAsync(id);

            return View(card);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CheckCardExistence, CheckCardOwner] int id)
        {
            await cardsService.DeleteCard(id);

            var card = await cardsService.FindCardAsync(id);

            return RedirectToAction("Details", "Decks", new { id = card.DeckID });
        }

        private async Task<ActionResult> PromoteAndRedirect(IDeck deck, Delays delay)
        {
            await schedulerService.PromoteCloze(deck, delay);

            var nextCard = deck.GetNextCard();

            return RedirectToAction("Details", new { id = nextCard.ID });
        }
    }
}
