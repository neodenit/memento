﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neodenit.Memento.Common;
using Neodenit.Memento.Common.Enums;
using Neodenit.Memento.Common.ViewModels;
using Neodenit.Memento.Services.API;
using Neodenit.Memento.Web.Attributes;
using Newtonsoft.Json;

namespace Neodenit.Memento.Web.Controllers
{
    [Authorize]
    public class CardsController : Controller
    {
        private readonly IDecksService decksService;
        private readonly ICardsService cardsService;
        private readonly IStatisticsService statService;
        private readonly ISchedulerService schedulerService;

        private string UserName => User.Identity.Name;

        public CardsController(IDecksService decksService, ICardsService cardsService, IStatisticsService statService, ISchedulerService schedulerService)
        {
            this.decksService = decksService;
            this.cardsService = cardsService;
            this.statService = statService;
            this.schedulerService = schedulerService;
        }

        [ValidateModel]
        public async Task<ActionResult> ClozesIndex([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            IEnumerable<ClozeViewModel> clozes = await decksService.GetClozesAsync(deckID, UserName);

            return View(clozes);
        }

        [ValidateModel]
        public async Task<ActionResult> CardsIndex([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            IEnumerable<ViewCardViewModel> cards = await decksService.GetCardsAsync(deckID);

            return View(cards);
        }

        [ValidateModel]
        public async Task<ActionResult> DeletedIndex([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            IEnumerable<ViewCardViewModel> deletedCards = await decksService.GetDeletedCardsAsync(deckID);

            return View(deletedCards);
        }

        [ValidateModel]
        public async Task<ActionResult> DraftIndex([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            IEnumerable<ViewCardViewModel> draftCards = await decksService.GetDraftCardsAsync(deckID);

            return View(draftCards);
        }

        [ValidateModel]
        public async Task<ActionResult> Details([CheckCardExistence, CheckCardOwner] Guid id)
        {
            ViewCardViewModel card = await cardsService.FindCardAsync(id);
            DeckViewModel deck = await decksService.FindDeckAsync(card.DeckID);
            ClozeViewModel cloze = await cardsService.GetNextClozeAsync(id, UserName);

            if (cloze.IsNew && deck.PreviewAnswer)
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

        [ValidateModel]
        public async Task<ActionResult> PreviewClosed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            AnswerCardViewModel viewModel = await cardsService.GetCardWithQuestionAsync(id, UserName);

            return View(viewModel);
        }

        [ValidateModel]
        public async Task<ActionResult> PreviewOpened([CheckCardExistence, CheckCardOwner] Guid id)
        {
            AnswerCardViewModel viewModel = await cardsService.GetCardWithAnswerAsync(id, UserName);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewOpened(ViewCardViewModel card)
        {

            ViewCardViewModel nextCard = await schedulerService.PromoteClozeAsync(card.ID, Delays.Same, UserName);

            return RedirectToCard(nextCard.ID);
        }

        [ValidateModel]
        public async Task<ActionResult> RepeatClosed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            AnswerCardViewModel viewModel = await cardsService.GetCardWithQuestionAsync(id, UserName);

            return View(viewModel);
        }

        [ValidateModel]
        public async Task<ActionResult> RepeatOpened([CheckCardExistence, CheckCardOwner] Guid id)
        {
            AnswerCardViewModel viewModel = await cardsService.GetCardWithAnswerAsync(id, UserName);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RepeatOpened(ViewCardViewModel card, string againButton, string badButton, string goodButton)
        {
            if (!Settings.Default.AllowSmoothDelayModes && badButton != null)
            {
                throw new Exception();
            }

            var isCorrect = goodButton != null;

            await statService.AddAnswer(card.ID, isCorrect, UserName);

            var delay = againButton != null
                ? Delays.Initial
                : badButton != null
                ? Delays.Previous
                : goodButton != null
                ? Delays.Next
                : Delays.Same;

            ViewCardViewModel nextCard = await schedulerService.PromoteClozeAsync(card.ID, delay, UserName);

            return RedirectToCard(nextCard.ID);
        }

        [ValidateModel]
        public async Task<ActionResult> Question([CheckCardExistence, CheckCardOwner] Guid id)
        {
            AnswerCardViewModel viewModel = await cardsService.GetCardWithQuestionAsync(id, UserName);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Question(AnswerCardViewModel card)
        {
            ViewCardViewModel dbCard = await cardsService.FindCardAsync(card.ID);
            AnswerCardViewModel evaluatedCard = await cardsService.EvaluateCardAsync(card.ID, card.UserAnswer, UserName);

            switch (evaluatedCard.Mark)
            {
                case Mark.Correct:
                    evaluatedCard.Statistics = await decksService.GetDeckWithStatViewModel(dbCard.DeckID, new StatisticsViewModel(), UserName);
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
        public async Task<ActionResult> Right(AnswerCardViewModel card)
        {
            await statService.AddAnswer(card.ID, true, UserName);

            ViewCardViewModel nextCard = await schedulerService.PromoteClozeAsync(card.ID, Delays.Next, UserName);

            return RedirectToCard(nextCard.ID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wrong(AnswerCardViewModel card, string NextButton, string AltButton)
        {
            if (NextButton != null)
            {
                await statService.AddAnswer(card.ID, false, UserName);

                ViewCardViewModel dbCard = await cardsService.FindCardAsync(card.ID);
                DeckViewModel deck = await decksService.FindDeckAsync(dbCard.DeckID);

                Delays delay = schedulerService.GetDelayForWrongAnswer(deck.DelayMode);

                ViewCardViewModel nextCard = await schedulerService.PromoteClozeAsync(card.ID, delay, UserName);

                return RedirectToCard(nextCard.ID);
            }
            else if (AltButton != null)
            {
                await cardsService.AddAltAnswerAsync(card.ID, card.UserAnswer, UserName);

                return RedirectToAction("Details", new { id = card.ID });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Typo(AnswerCardViewModel card)
        {
            return await Task.FromResult(RedirectToAction("Details", new { card.ID }));
        }

        public async Task<ActionResult> Create(Guid? DeckID, Guid? readingCardId, Guid? repetitionCardId, string text)
        {
            if (DeckID == null)
            {
                ViewBag.Decks = new SelectList(await decksService.GetDecksAsync(User.Identity.Name), "ID", "Title");

                var card = new EditCardViewModel
                {
                    ID = repetitionCardId.GetValueOrDefault(),
                    ReadingCardId = readingCardId.GetValueOrDefault(),
                    DeckID = Guid.Empty,
                    Text = text
                };

                return View(card);
            }
            else
            {
                DeckViewModel deck = await decksService.FindDeckAsync(DeckID.Value);

                var card = new EditCardViewModel { DeckID = deck.ID, DeckTitle = deck.Title };

                return View(card);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EditCardViewModel card)
        {
            if (card.ReadingCardId != Guid.Empty)
            {
                bool isCardIdValid = await IsReadingCardValidAsync(card.ID, card.ReadingCardId);

                if (!isCardIdValid)
                {
                    return BadRequest();
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

        [ValidateModel]
        public async Task<ActionResult> Edit([CheckCardExistence, CheckCardOwner] Guid id)
        {
            EditCardViewModel card = await cardsService.FindEditCardAsync(id);
            DeckViewModel deck = await decksService.FindDeckAsync(card.DeckID);

            card.DeckTitle = deck.Title;

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditCardViewModel card)
        {
            if (ModelState.IsValid)
            {
                await cardsService.UpdateCard(card);

                ViewCardViewModel dbCard = await cardsService.FindCardAsync(card.ID);

                return RedirectToAction("Details", "Decks", new { id = dbCard.DeckID });
            }
            else
            {
                return View(card);
            }
        }

        public async Task<ActionResult> ShuffleNewCards(Guid cardID)
        {
            ViewCardViewModel card = await cardsService.FindCardAsync(cardID);

            return await ShuffleNew(card.DeckID);
        }

        [ValidateModel]
        public async Task<ActionResult> ShuffleNew([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            await schedulerService.ShuffleNewClozes(deckID, UserName);

            return RedirectToAction("ClozesIndex", new { deckID });
        }

        [ValidateModel]
        public async Task<ActionResult> Restore([CheckCardExistence, CheckCardOwner] Guid id)
        {
            ViewCardViewModel card = await cardsService.FindCardAsync(id);

            return View(card);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        [ValidateModel]
        public async Task<ActionResult> RestoreConfirmed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            await cardsService.RestoreCard(id);

            ViewCardViewModel card = await cardsService.FindCardAsync(id);

            return RedirectToAction("DeletedIndex", "Cards", new { DeckID = card.DeckID });
        }

        public async Task<ActionResult> Delete([CheckCardExistence, CheckCardOwner] Guid id)
        {
            ViewCardViewModel card = await cardsService.FindCardAsync(id);

            return View(card);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidateModel]
        public async Task<ActionResult> DeleteConfirmed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            await cardsService.DeleteCard(id);

            ViewCardViewModel card = await cardsService.FindCardAsync(id);

            return RedirectToAction("Details", "Decks", new { id = card.DeckID });
        }

        [AllowAnonymous]
        public async Task<ActionResult> IsValid(Guid readingCardId, Guid repetitionCardId)
        {
            bool isValid = await cardsService.IsCardValidAsync(readingCardId, repetitionCardId);
            return Json(isValid);
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
