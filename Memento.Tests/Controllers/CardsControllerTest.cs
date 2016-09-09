using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Models.ViewModels;
using Memento.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Memento.Tests.Controllers
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

        [TestInitialize]
        public void Setup()
        {
            mockDecksService = new Mock<IDecksService>();
            mockCardsService = new Mock<ICardsService>();
            mockStatService = new Mock<IStatisticsService>();
            mockSchedulerService = new Mock<ISchedulerService>();

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(item => item.HttpContext.User.Identity.Name).Returns(userName);

            var mockDeck = new Mock<IDeck>();
            var mockCard = new Mock<ICard>();
            var mockCloze = new Mock<ICloze>();

            var mockAnswerCardViewModel = new Mock<IAnswerCardViewModel>();

            mockDecksService.Setup(x => x.FindDeckAsync(It.IsAny<int>())).ReturnsAsync(mockDeck.Object);
            mockCardsService.Setup(x => x.FindCardAsync(It.IsAny<int>())).ReturnsAsync(mockCard.Object);
            mockCardsService.Setup(x => x.GetCardWithQuestion(It.IsAny<ICloze>())).Returns(mockAnswerCardViewModel.Object);
            mockCardsService.Setup(x => x.GetCardWithAnswer(It.IsAny<ICloze>())).Returns(mockAnswerCardViewModel.Object);
            mockCardsService.Setup(x => x.EvaluateCard(It.IsAny<ICloze>(), It.IsAny<string>())).Returns(mockAnswerCardViewModel.Object);
            mockSchedulerService.Setup(x => x.GetDelayForWrongAnswer(It.IsAny<DelayModes>()));


            mockDeck.Setup(x => x.GetNextCard(It.IsAny<string>())).Returns(mockCard.Object);

            mockCard.Setup(x => x.GetNextCloze(It.IsAny<string>())).Returns(mockCloze.Object);
            mockCard.Setup(x => x.GetDeck()).Returns(mockDeck.Object);

            sut = new CardsController(mockDecksService.Object, mockCardsService.Object, mockStatService.Object, mockSchedulerService.Object)
            {
                ControllerContext = mockContext.Object
            };
        }

        [TestMethod()]
        public async Task CardsClozesIndexTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.ClozesIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<ClozeViewModel>;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsCardsIndexTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.CardsIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<ViewCardViewModel>;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsDeletedIndexTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.DeletedIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<ViewCardViewModel>;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsDraftIndexTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.DraftIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<ViewCardViewModel>;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsDetailsTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Details(id) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsPreviewClosedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.PreviewClosed(id) as ViewResult;
            var model = result.Model as IAnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            mockCardsService.Verify(x => x.GetCardWithQuestion(It.IsAny<ICloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsPreviewOpenedGetTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.PreviewOpened(id) as ViewResult;
            var model = result.Model as IAnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.GetCardWithAnswer(It.IsAny<ICloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsPreviewOpenedPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = 1 };

            // Act
            var result = await sut.PreviewOpened(card) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<IDeck>(), Delays.Same, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRepeatClosedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.RepeatClosed(id) as ViewResult;
            var model = result.Model as IAnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.GetCardWithQuestion(It.IsAny<ICloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedGetTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.RepeatOpened(id) as ViewResult;
            var model = result.Model as IAnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.GetCardWithAnswer(It.IsAny<ICloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedAgainButtonPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = 1 };

            // Act
            var result = await sut.RepeatOpened(card, "againButton", null, null) as RedirectToRouteResult;

            // Assert
            mockStatService.Verify(x => x.AddAnswer(card.ID, It.IsAny<bool>(), It.IsAny<string>()));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<IDeck>(), Delays.Initial, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedBadButtonPostTest()
        {
            if (Settings.Default.AllowSmoothDelayModes)
            {

                // Arrange
                var card = new ViewCardViewModel { ID = 1 };

                // Act
                var result = await sut.RepeatOpened(card, null, "badButton", null) as RedirectToRouteResult;

                // Assert
                mockStatService.Verify(x => x.AddAnswer(card.ID, It.IsAny<bool>(), It.IsAny<string>()));
                mockCardsService.Verify(x => x.FindCardAsync(card.ID));
                mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<IDeck>(), Delays.Previous, It.IsAny<string>()));
                Assert.IsNotNull(result);
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(Exception))]
        public async Task CardsRepeatOpenedBadButtonPostExceptionTest()
        {
            if (!Settings.Default.AllowSmoothDelayModes)
            {
                // Arrange
                var card = new ViewCardViewModel { ID = 1 };

                // Act
                var result = await sut.RepeatOpened(card, null, "badButton", null) as RedirectToRouteResult;
            }
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedGoodButtonPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = 1 };

            // Act
            var result = await sut.RepeatOpened(card, null, null, "goodButton") as RedirectToRouteResult;

            // Assert
            mockStatService.Verify(x => x.AddAnswer(card.ID, It.IsAny<bool>(), It.IsAny<string>()));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<IDeck>(), Delays.Next, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsQuestionTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Question(id) as ViewResult;
            var model = result.Model as IAnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.GetCardWithQuestion(It.IsAny<ICloze>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsQuestionPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel();

            // Act
            var result = await sut.Question(card) as ViewResult;
            var model = result.Model as IAnswerCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.EvaluateCard(It.IsAny<ICloze>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsRightTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Right(card) as RedirectToRouteResult;

            // Assert
            mockStatService.Verify(x => x.AddAnswer(card.ID, It.IsAny<bool>(), It.IsAny<string>()));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<IDeck>(), Delays.Next, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsWrongNextButtonPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Wrong(card, "NextButton", null) as RedirectToRouteResult;

            // Assert
            mockStatService.Verify(x => x.AddAnswer(card.ID, false, It.IsAny<string>()));
            mockSchedulerService.Verify(x => x.GetDelayForWrongAnswer(It.IsAny<DelayModes>()));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            mockSchedulerService.Verify(x => x.PromoteCloze(It.IsAny<IDeck>(), It.IsAny<Delays>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsWrongAltButtonPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Wrong(card, null, "AltButton") as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.AddAltAnswer(It.IsAny<ICloze>(), card.UserAnswer));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsTypoPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Typo(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsCreateTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Create(id) as ViewResult;
            var model = result.Model as EditCardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.DeckID);
        }

        [TestMethod()]
        public async Task CardsCreateNullDeckIDTest()
        {
            // Arrange
            int? id = null;

            // Act
            var result = await sut.Create(id) as ViewResult;
            var model = result.Model as EditCardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(-1, model.DeckID);
        }

        [TestMethod()]
        public async Task CardsCreatePostTest()
        {
            // Arrange
            var id = 1;
            var card = new EditCardViewModel { ID = id };

            // Act
            var result = await sut.Create(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsEditTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Edit(id) as ViewResult;
            var model = result.Model as EditCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsEditPostTest()
        {
            // Arrange
            var card = new EditCardViewModel { ID = 1, Text = "text", Comment = "comment" };

            // Act
            var result = await sut.Edit(card) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.UpdateCard(card));
            mockCardsService.Verify(x => x.FindCardAsync(card.ID));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsShuffleNewCardsTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.ShuffleNewCards(id) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            mockSchedulerService.Verify(x => x.ShuffleNewClozes(It.IsAny<int>(), It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsShuffleNewTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.ShuffleNew(id) as RedirectToRouteResult;

            // Assert
            mockSchedulerService.Verify(x => x.ShuffleNewClozes(id, It.IsAny<string>()));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRestoreTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Restore(id) as ViewResult;
            var model = result.Model as ICard;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsRestoreConfirmedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.RestoreConfirmed(id) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.RestoreCard(id));
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsDeleteTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Delete(id) as ViewResult;
            var model = result.Model as ViewCardViewModel;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task CardsDeleteConfirmedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.DeleteConfirmed(id) as RedirectToRouteResult;

            // Assert
            mockCardsService.Verify(x => x.FindCardAsync(id));
            Assert.IsNotNull(result);
        }
    }
}