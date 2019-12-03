using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neodenit.Memento.Additional;
using Neodenit.Memento.Common;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.DataModels;
using Neodenit.Memento.Models.Enums;
using Neodenit.Memento.Models.ViewModels;
using Neodenit.Memento.Web.Controllers;

namespace Neodenit.Memento.Tests.Controllers
{
    [TestClass()]
    public class CardsControllerTest
    {
        private CardsController sut;

        private Mock<IDecksService> mockDecksService;
        private Mock<ICardsService> mockCardsService;
        private Mock<IStatisticsService> mockStatService;
        private Mock<ISchedulerService> mockSchedulerService;

        private string userName = "user@server.com";

        private Guid deckId = new Guid("00000000-0000-0000-0000-000000000010");
        private Guid cardId = new Guid("00000000-0000-0000-0000-000000000020");
        private Guid clozeId = new Guid("00000000-0000-0000-0000-000000000030");
        private Guid repetitionId = new Guid("00000000-0000-0000-0000-000000000040");

        [TestInitialize]
        public void Setup()
        {
            mockDecksService = new Mock<IDecksService>();
            mockCardsService = new Mock<ICardsService>();
            mockStatService = new Mock<IStatisticsService>();
            mockSchedulerService = new Mock<ISchedulerService>();

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(item => item.HttpContext.User.Identity.Name).Returns(userName);

            var repetition = new UserRepetition { ID = repetitionId, ClozeID = clozeId };
            var cloze = new Cloze { UserRepetitions = new[] { repetition }, ID = clozeId, CardID = cardId };
            var card = new Card { Clozes = new[] { cloze }, ID = cardId, DeckID = deckId, IsValid = true };
            var deck = new Deck { Cards = new[] { card }, ID = deckId };

            repetition.Cloze = cloze;
            cloze.Card = card;
            card.Deck = deck;

            var mockAnswerCardViewModel = new Mock<AnswerCardViewModel>();

            mockDecksService.Setup(x => x.FindDeckAsync(It.IsAny<Guid>())).ReturnsAsync(deck);
            mockCardsService.Setup(x => x.FindCardAsync(It.IsAny<Guid>())).ReturnsAsync(card);
            mockCardsService.Setup(x => x.GetCardWithQuestion(It.IsAny<Cloze>())).Returns(mockAnswerCardViewModel.Object);
            mockCardsService.Setup(x => x.GetCardWithAnswer(It.IsAny<Cloze>())).Returns(mockAnswerCardViewModel.Object);
            mockCardsService.Setup(x => x.EvaluateCard(It.IsAny<Cloze>(), It.IsAny<string>())).Returns(mockAnswerCardViewModel.Object);
            mockSchedulerService.Setup(x => x.GetDelayForWrongAnswer(It.IsAny<DelayModes>()));

            sut = new CardsController(mockDecksService.Object, mockCardsService.Object, mockStatService.Object, mockSchedulerService.Object)
            {
                ControllerContext = mockContext.Object
            };
        }

        [TestMethod]
        public async Task CardsClozesIndexTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.ClozesIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<ClozeViewModel>;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsCardsIndexTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.CardsIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<ViewCardViewModel>;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsDeletedIndexTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.DeletedIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<ViewCardViewModel>;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsDraftIndexTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.DraftIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<ViewCardViewModel>;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsDetailsTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.Details(id) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsPreviewClosedTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.PreviewClosed(id) as ViewResult;
            var model = result.Model as AnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            mockCardsService.Verify(x => x.GetCardWithQuestion(It.IsAny<Cloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsPreviewOpenedGetTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.PreviewOpened(id) as ViewResult;
            var model = result.Model as AnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.GetCardWithAnswer(It.IsAny<Cloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsPreviewOpenedPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = cardId };

            // Act
            var result = await sut.PreviewOpened(card) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<Deck>(), Delays.Same, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsRepeatClosedTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.RepeatClosed(id) as ViewResult;
            var model = result.Model as AnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.GetCardWithQuestion(It.IsAny<Cloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsRepeatOpenedGetTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.RepeatOpened(id) as ViewResult;
            var model = result.Model as AnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.GetCardWithAnswer(It.IsAny<Cloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsRepeatOpenedAgainButtonPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = cardId };

            // Act
            var result = await sut.RepeatOpened(card, "againButton", null, null) as RedirectToRouteResult;

            // Assert
            mockStatService.Verify(x => x.AddAnswer(card.ID, It.IsAny<bool>(), It.IsAny<string>()));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<Deck>(), Delays.Initial, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsRepeatOpenedBadButtonPostTest()
        {
            if (Settings.Default.AllowSmoothDelayModes)
            {

                // Arrange
                var card = new ViewCardViewModel { ID = cardId };

                // Act
                var result = await sut.RepeatOpened(card, null, "badButton", null) as RedirectToRouteResult;

                // Assert
                mockStatService.Verify(x => x.AddAnswer(card.ID, It.IsAny<bool>(), It.IsAny<string>()));
                mockCardsService.Verify(x => x.FindCardAsync(card.ID));
                mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<Deck>(), Delays.Previous, It.IsAny<string>()));
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CardsRepeatOpenedBadButtonPostExceptionTest()
        {
            if (!Settings.Default.AllowSmoothDelayModes)
            {
                // Arrange
                var card = new ViewCardViewModel { ID = cardId };

                // Act
                var result = await sut.RepeatOpened(card, null, "badButton", null) as RedirectToRouteResult;
            }
        }

        [TestMethod]
        public async Task CardsRepeatOpenedGoodButtonPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = cardId };

            // Act
            var result = await sut.RepeatOpened(card, null, null, "goodButton") as RedirectToRouteResult;

            // Assert
            mockStatService.Verify(x => x.AddAnswer(card.ID, It.IsAny<bool>(), It.IsAny<string>()));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<Deck>(), Delays.Next, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsQuestionTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.Question(id) as ViewResult;
            var model = result.Model as AnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.GetCardWithQuestion(It.IsAny<Cloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task Question_ArgumentIsOk_VerifiesCard()
        {
            // Arrange
            var card = new AnswerCardViewModel();

            // Act
            var result = await sut.Question(card) as ViewResult;
            var model = result.Model as AnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.EvaluateCard(It.IsAny<Cloze>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task Question_AnswerIsRight_GetsStatistics()
        {
            // Arrange
            var card = new AnswerCardViewModel();
            mockCardsService
                .Setup(m => m.EvaluateCard(It.IsAny<Cloze>(), It.IsAny<string>()))
                .Returns(new AnswerCardViewModel { Mark = Mark.Correct });

            // Act
            var result = await sut.Question(card) as ViewResult;

            // Assert
            mockDecksService.Verify(x => x.GetDeckWithStatViewModel(It.IsAny<Guid>(), It.IsAny<Statistics>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ViewName, "Right");
        }

        [TestMethod]
        public async Task CardsRightTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = cardId };

            // Act
            var result = await sut.Right(card) as RedirectToRouteResult;

            // Assert
            mockStatService.Verify(x => x.AddAnswer(card.ID, It.IsAny<bool>(), It.IsAny<string>()));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<Deck>(), Delays.Next, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsWrongNextButtonPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = cardId };

            // Act
            var result = await sut.Wrong(card, "NextButton", null) as RedirectToRouteResult;

            // Assert
            mockStatService.Verify(x => x.AddAnswer(card.ID, false, It.IsAny<string>()));
            mockSchedulerService.Verify(x => x.GetDelayForWrongAnswer(It.IsAny<DelayModes>()));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<Deck>(), It.IsAny<Delays>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsWrongAltButtonPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = cardId };

            // Act
            var result = await sut.Wrong(card, null, "AltButton") as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.AddAltAnswer(It.IsAny<Cloze>(), card.UserAnswer));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsTypoPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = cardId };

            // Act
            var result = await sut.Typo(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsCreateTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.Create(id, null, null, null) as ViewResult;
            var model = result.Model as EditCardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.DeckID);
        }

        [TestMethod]
        public async Task CardsCreateNullDeckIDTest()
        {
            // Arrange
            Guid? id = null;

            // Act
            var result = await sut.Create(id, null, null, null) as ViewResult;
            var model = result.Model as EditCardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(Guid.Empty, model.DeckID);
        }

        [TestMethod]
        public async Task CardsCreatePostTest()
        {
            // Arrange
            var id = cardId;
            var card = new EditCardViewModel { ID = id };

            // Act
            var result = await sut.Create(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsEditTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.Edit(id) as ViewResult;
            var model = result.Model as EditCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsEditPostTest()
        {
            // Arrange
            var card = new EditCardViewModel { ID = cardId, Text = "text", Comment = "comment" };

            // Act
            var result = await sut.Edit(card) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.UpdateCard(card));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsShuffleNewCardsTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.ShuffleNewCards(id) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            mockSchedulerService.Verify(x => x.ShuffleNewClozes(It.IsAny<Guid>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsShuffleNewTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.ShuffleNew(id) as RedirectToRouteResult;

            // Assert
            mockSchedulerService.Verify(x => x.ShuffleNewClozes(id, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsRestoreTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.Restore(id) as ViewResult;
            var model = result.Model as Card;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsRestoreConfirmedTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.RestoreConfirmed(id) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.RestoreCard(id));
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CardsDeleteTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.Delete(id) as ViewResult;
            var model = result.Model as ViewCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task CardsDeleteConfirmedTest()
        {
            // Arrange
            var id = cardId;

            // Act
            var result = await sut.DeleteConfirmed(id) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
        }
    }
}