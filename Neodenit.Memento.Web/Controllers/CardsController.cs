using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neodenit.Memento.Attributes;
using Neodenit.Memento.Common;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.Enums;
using Neodenit.Memento.Models.ViewModels;
using Newtonsoft.Json;

namespace Neodenit.Memento.Web.Controllers
{
    [Authorize]
    public class CardsController : Controller
    {
        private readonly IMapper mapper;
        private readonly IDecksService decksService;
        private readonly ICardsService cardsService;
        private readonly IStatisticsService statService;
        private readonly ISchedulerService schedulerService;

        private string UserName => User.Identity.Name;

        public CardsController(IMapper mapper, IDecksService decksService, ICardsService cardsService, IStatisticsService statService, ISchedulerService schedulerService)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.decksService = decksService;
            this.cardsService = cardsService;
            this.statService = statService;
            this.schedulerService = schedulerService;
        }

        public async Task<ActionResult> ClozesIndex([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var clozes = deck.GetClozes();
            var orderedClozes = clozes.OrderBy(cloze => cloze.GetUserRepetition(UserName).Position);

            var viewModel = from cloze in orderedClozes
                            select mapper.Map<Cloze, ClozeViewModel>(cloze, opt =>
                                opt.AfterMap((src, dest) =>
                                {
                                    dest.Position = cloze.GetUserRepetition(UserName).Position;
                                    dest.IsNew = cloze.GetUserRepetition(UserName).IsNew;
                                    dest.LastDelay = cloze.GetUserRepetition(UserName).LastDelay;
                                }));

            return View(viewModel);
        }

        public async Task<ActionResult> CardsIndex([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var cards = deck.GetValidCards();

            var viewModel = mapper.Map<IEnumerable<ViewCardViewModel>>(cards);

            return View(viewModel);
        }

        public async Task<ActionResult> DeletedIndex([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var cards = deck.GetDeletedCards();
            var viewModel = mapper.Map<IEnumerable<ViewCardViewModel>>(cards);

            return View(viewModel);
        }

        public async Task<ActionResult> DraftIndex([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            var deck = await decksService.FindDeckAsync(deckID);
            var cards = deck.GetDraftCards();
            var viewModel = mapper.Map<IEnumerable<ViewCardViewModel>>(cards);

            return View(viewModel);
        }

        public async Task<ActionResult> Details([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var deck = card.Deck;
            var cloze = card.GetNextCloze(UserName);

            if (cloze.GetUserRepetition(UserName).IsNew && deck.PreviewAnswer)
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
            var cloze = card.GetNextCloze(UserName);
            var viewModel = cardsService.GetCardWithQuestion(cloze);

            return View(viewModel);
        }

        public async Task<ActionResult> PreviewOpened([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(UserName);
            var viewModel = cardsService.GetCardWithAnswer(cloze);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewOpened(ViewCardViewModel card)
        {
            var dbCard = await cardsService.FindCardAsync(card.ID);
            var deck = dbCard.Deck;

            await schedulerService.PromoteCloze(deck, Delays.Same, UserName);

            var nextCard = deck.GetNextCard(UserName);

            return RedirectToCard(nextCard.ID);
        }

        public async Task<ActionResult> RepeatClosed([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(UserName);
            var viewModel = cardsService.GetCardWithQuestion(cloze);

            return View(viewModel);
        }

        public async Task<ActionResult> RepeatOpened([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(UserName);
            var viewModel = cardsService.GetCardWithAnswer(cloze);

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

            var delay = againButton != null ?
                        Delays.Initial :
                        badButton != null ?
                        Delays.Previous :
                        goodButton != null ?
                        Delays.Next :
                        Delays.Same;

            var dbCard = await cardsService.FindCardAsync(card.ID);
            var deck = dbCard.Deck;

            await schedulerService.PromoteCloze(deck, delay, UserName);

            var nextCard = deck.GetNextCard(UserName);

            return RedirectToCard(nextCard.ID);
        }

        public async Task<ActionResult> Question([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);
            var cloze = card.GetNextCloze(UserName);
            var viewModel = cardsService.GetCardWithQuestion(cloze);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Question(AnswerCardViewModel card)
        {
            var dbCard = await cardsService.FindCardAsync(card.ID);
            var cloze = dbCard.GetNextCloze(UserName);

            var evaluatedCard = cardsService.EvaluateCard(cloze, card.UserAnswer);

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

            var dbCard = await cardsService.FindCardAsync(card.ID);
            var deck = dbCard.Deck;

            await schedulerService.PromoteCloze(deck, Delays.Next, UserName);

            var nextCard = deck.GetNextCard(UserName);

            return RedirectToCard(nextCard.ID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wrong(AnswerCardViewModel card, string NextButton, string AltButton)
        {
            if (NextButton != null)
            {
                await statService.AddAnswer(card.ID, false, UserName);

                var dbCard = await cardsService.FindCardAsync(card.ID);
                var deck = dbCard.Deck;
                var delay = schedulerService.GetDelayForWrongAnswer(deck.DelayMode);

                await schedulerService.PromoteCloze(deck, delay, UserName);

                var nextCard = deck.GetNextCard(UserName);

                return RedirectToCard(nextCard.ID);
            }
            else if (AltButton != null)
            {
                var dbCard = await cardsService.FindCardAsync(card.ID);
                var close = dbCard.GetNextCloze(UserName);

                await cardsService.AddAltAnswer(close, card.UserAnswer);

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
                var card = new EditCardViewModel { DeckID = DeckID.Value };

                return View(card);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EditCardViewModel card)
        {
            if (card.ReadingCardId != Guid.Empty)
            {
                var isCardIdValid = await IsReadingCardValidAsync(card.ID, card.ReadingCardId);

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
        public async Task<ActionResult> Edit([CheckCardExistence, CheckCardOwner] Guid id)
        {
            var card = await cardsService.FindCardAsync(id);

            var viewModel = mapper.Map<EditCardViewModel>(card);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditCardViewModel card)
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

        public async Task<ActionResult> ShuffleNew([CheckDeckExistence, CheckDeckOwner] Guid deckID)
        {
            await schedulerService.ShuffleNewClozes(deckID, UserName);

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
            var viewModel = mapper.Map<ViewCardViewModel>(card);

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
