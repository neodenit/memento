﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Memento.Additional;
using Memento.Attributes;
using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Enums;
using Memento.Models.Models;
using Memento.Models.ViewModels;
using Newtonsoft.Json;

namespace Memento.Web.Controllers
{
    [Authorize]
    [RequireHttps]
    public class CardsController : Controller
    {
        private readonly IDecksService decksService;
        private readonly ICardsService cardsService;
        private readonly IStatisticsService statService;
        private readonly ISchedulerService schedulerService;

        private string username;

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

        public async Task<ActionResult> Details([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var deck = card.GetDeck();
            var cloze = card.GetNextCloze(username);

            if (cloze.GetUserRepetition(username).IsNew && deck.PreviewAnswer)
            {
                return RedirectToAction("PreviewClosed", new { card.ID });
            }
            else if (deck.ControlMode == ControlModes.Manual)
            {
                return RedirectToAction("RepeatClosed", new { card.ID });
            }
            else if (deck.ControlMode == ControlModes.Automatic)
            {
                return RedirectToAction("Question", new { card.ID });
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task<ActionResult> PreviewClosed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(username);
            var viewModel = cardsService.GetCardWithQuestion(cloze);

            return View(viewModel);
        }

        public async Task<ActionResult> PreviewOpened([CheckCardExistence, CheckCardOwner] Guid id)
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

        public async Task<ActionResult> RepeatClosed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(username);
            var viewModel = cardsService.GetCardWithQuestion(cloze);

            return View(viewModel);
        }

        public async Task<ActionResult> RepeatOpened([CheckCardExistence, CheckCardOwner] Guid id)
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

        public async Task<ActionResult> Question([CheckCardExistence, CheckCardOwner] Guid id)
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
                    evaluatedCard.Statistics = await decksService.GetDeckWithStatViewModel(dbCard.DeckID, new Statistics(), username);
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

        public async Task<ActionResult> Create(int? DeckID, Guid? readingCardId, Guid? repetitionCardId, string text)
        {
            if (DeckID == null)
            {
                ViewBag.Decks = new SelectList(await decksService.GetDecksAsync(User.Identity.Name), "ID", "Title");

                var card = new EditCardViewModel
                {
                    ID = repetitionCardId.GetValueOrDefault(),
                    ReadingCardId = readingCardId.GetValueOrDefault(),
                    DeckID = -1,
                    Text = text
                };

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
        public async Task<ActionResult> Create([Bind(Include = "ID, DeckID, ReadingCardId, Text, Comment")] EditCardViewModel card)
        {
            if (card.ReadingCardId != Guid.Empty)
            {
                var isCardIdValid = await IsReadingCardValidAsync(card.ID, card.ReadingCardId);

                if (!isCardIdValid)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            if (ModelState.IsValid)
            {
                await cardsService.AddCard(card);

                if (card.ReadingCardId == Guid.Empty)
                {
                    return RedirectToAction("Create", "Cards", new { card.DeckID });
                }
                else
                {
                    var baseUri = new Uri(Settings.Default.IncrementalReadingServer);
                    var uri = new Uri(baseUri, $"Cards/Details/{card.ReadingCardId}");
                    return Redirect(uri.ToString());
                }
            }
            else
            {
                ViewBag.Decks = new SelectList(await decksService.GetDecksAsync(User.Identity.Name), "ID", "Title");
                return View(card);
            }
        }
        public async Task<ActionResult> Edit([CheckCardExistence, CheckCardOwner] Guid id)
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

        public async Task<ActionResult> ShuffleNewCards(Guid cardID)
        {
            var card = await cardsService.FindCardAsync(cardID);

            return await ShuffleNew(card.DeckID);
        }

        public async Task<ActionResult> ShuffleNew([CheckDeckExistence, CheckDeckOwner] int deckID)
        {
            await schedulerService.ShuffleNewClozes(deckID, username);

            return RedirectToAction("ClozesIndex", new { deckID });
        }

        public async Task<ActionResult> Restore([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);

            return View(card);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreConfirmed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            await cardsService.RestoreCard(id);

            var card = await cardsService.FindCardAsync(id);

            return RedirectToAction("DeletedIndex", "Cards", new { DeckID = card.DeckID });
        }

        public async Task<ActionResult> Delete([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var viewModel = new ViewCardViewModel(card);

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            await cardsService.DeleteCard(id);

            var card = await cardsService.FindCardAsync(id);

            return RedirectToAction("Details", "Decks", new { id = card.DeckID });
        }

        [AllowAnonymous]
        public async Task<ActionResult> IsValid(Guid readingCardId, Guid repetitionCardId)
        {
            bool isValid = await cardsService.IsCardValidAsync(readingCardId, repetitionCardId);
            return Json(isValid, JsonRequestBehavior.AllowGet);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            username = User.Identity.Name;
        }

        private ActionResult RedirectToNextCard(Deck deck)
        {
            var nextCard = deck.GetNextCard(username);

            return RedirectToCard(nextCard.ID);
        }

        private ActionResult RedirectToCard(Guid cardID)
        {
            return RedirectToAction("Details", new { id = cardID });
        }

        private async Task<bool> IsReadingCardValidAsync(Guid repetitionCardId, Guid readingCardId)
        {
            var baseUri = new Uri(Settings.Default.IncrementalReadingServer);
            var uri = new Uri(baseUri, $"Cards/IsValid?readingCardId={readingCardId}&repetitionCardId={repetitionCardId}");
            var response = await HomeController.HttpClient.GetAsync(uri);
            var responseString = await response.Content.ReadAsStringAsync();
            var isValid = JsonConvert.DeserializeObject<bool>(responseString);
            return isValid;
        }
    }
}
