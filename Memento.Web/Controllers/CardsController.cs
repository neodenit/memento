using Memento.Attributes;
using Memento.Common;
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

        private readonly string username;

        public CardsController(IDecksService decksService, ICardsService cardsService, IStatisticsService statService, ISchedulerService schedulerService)
        {
            this.decksService = decksService;
            this.cardsService = cardsService;
            this.statService = statService;
            this.schedulerService = schedulerService;

            username = User.Identity.Name;
        }

        public async Task<ActionResult> ClozesIndex([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var clozes = deck.GetClozes();
            var orderedClozes = clozes.OrderBy(cloze => cloze.GetUserRepetition(username).Position);
            var clozeViews = from cloze in orderedClozes select new ClozeViewModel(cloze, username);

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
            var cloze = card.GetNextCloze(username);

            if (cloze.GetUserRepetition(username).IsNew)
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
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(username);
            var viewModel = cardsService.GetCardWithQuestion(cloze);

            return View(viewModel);
        }

        public async Task<ActionResult> PreviewOpened([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(username);
            var viewModel = cardsService.GetCardWithAnswer(cloze);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewOpened([Bind(Include = "ID")] ViewCardViewModel card)
        {
            var dbCard = await cardsService.FindCardAsync(card.ID);
            var deck = dbCard.GetDeck();

            await schedulerService.PromoteCloze(deck, Delays.Same, username);

            return RedirectToNextCard(deck);
        }

        public async Task<ActionResult> RepeatClosed([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(username);
            var viewModel = cardsService.GetCardWithQuestion(cloze);

            return View(viewModel);
        }

        public async Task<ActionResult> RepeatOpened([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(username);
            var viewModel = cardsService.GetCardWithAnswer(cloze);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RepeatOpened([Bind(Include = "ID")] ViewCardViewModel card, string againButton, string badButton, string goodButton)
        {
            if (!Settings.Default.AllowSmoothDelayModes && badButton != null)
            {
                throw new Exception();
            }

            var isCorrect = goodButton != null;

            await statService.AddAnswer(card.ID, isCorrect, username);

            var delay = againButton != null ?
                        Delays.Initial :
                        badButton != null ?
                        Delays.Previous :
                        goodButton != null ?
                        Delays.Next :
                        Delays.Same;

            var dbCard = await cardsService.FindCardAsync(card.ID);
            var deck = dbCard.GetDeck();

            await schedulerService.PromoteCloze(deck, delay, username);

            return RedirectToNextCard(deck);
        }

        public async Task<ActionResult> Question([CheckCardExistence, CheckCardOwner] int id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(username);
            var viewModel = cardsService.GetCardWithQuestion(cloze);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Question([Bind(Include = "ID, UserAnswer")] AnswerCardViewModel card)
        {
            var dbCard = await cardsService.FindCardAsync(card.ID);
            var cloze = dbCard.GetNextCloze(username);

            var evaluatedCard = cardsService.EvaluateCard(cloze, card.UserAnswer);

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
            await statService.AddAnswer(card.ID, true, username);

            var dbCard = await cardsService.FindCardAsync(card.ID);
            var deck = dbCard.GetDeck();

            await schedulerService.PromoteCloze(deck, Delays.Next, username);

            return RedirectToNextCard(deck);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wrong([Bind(Include = "ID, UserAnswer")] AnswerCardViewModel card, string NextButton, string AltButton)
        {
            if (NextButton != null)
            {
                await statService.AddAnswer(card.ID, false, username);

                var dbCard = await cardsService.FindCardAsync(card.ID);
                var deck = dbCard.GetDeck();
                var delay = schedulerService.GetDelayForWrongAnswer(deck.DelayMode);

                await schedulerService.PromoteCloze(deck, delay, username);

                return RedirectToNextCard(deck);
            }
            else if (AltButton != null)
            {
                var dbCard = await cardsService.FindCardAsync(card.ID);
                var close = dbCard.GetNextCloze(username);

                await cardsService.AddAltAnswer(close, card.UserAnswer);

                return RedirectToAction("Details", new { id = card.ID });
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Typo([Bind(Include = "ID")] AnswerCardViewModel card)
        {
            return await Task.FromResult(RedirectToAction("Details", new { card.ID }));
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
        public async Task<ActionResult> Create([Bind(Include = "ID, DeckID, Text, Comment")] EditCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                await cardsService.AddCard(card);

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
        public async Task<ActionResult> Edit([Bind(Include = "ID, Text, Comment")] EditCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                await cardsService.UpdateCard(card);

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
            await schedulerService.ShuffleNewClozes(deckID, username);

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
            var viewModel = new ViewCardViewModel(card);

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CheckCardExistence, CheckCardOwner] int id)
        {
            await cardsService.DeleteCard(id);

            var card = await cardsService.FindCardAsync(id);

            return RedirectToAction("Details", "Decks", new { id = card.DeckID });
        }

        private ActionResult RedirectToNextCard(IDeck deck)
        {
            var nextCard = deck.GetNextCard(username);

            return RedirectToCard(nextCard.ID);
        }

        private ActionResult RedirectToCard(int cardID)
        {
            return RedirectToAction("Details", new { id = cardID });
        }
    }
}
