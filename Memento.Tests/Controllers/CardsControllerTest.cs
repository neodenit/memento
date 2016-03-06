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

        private List<Deck> decks;
        private List<Card> validCards;
        private List<Card> deletedCards;
        private List<Card> draftCards;
        private List<Cloze> clozes;
        private List<Answer> answers;

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

            sut = new CardsController(mockDecksService.Object, mockCardsService.Object, mockStatService.Object, mockSchedulerService.Object)
            {
                ControllerContext = mockContext.Object
            };
        }

        private void AddFakeRepository()
        {
            var firstCardClozes = new List<Cloze> {
                new Cloze { ID = 1, IsNew = true },
                new Cloze { ID = 2 },
                new Cloze { ID = 3 },
            };
            var secondCardClozes = new List<Cloze> {
                new Cloze { ID = 4 },
                new Cloze { ID = 5 },
            };

            validCards = new List<Card> {
                new Card {
                    ID = 1,
                    IsValid = true,
                    Clozes = firstCardClozes,
                    Deck = new Deck {
                        ControlMode = ControlModes.Automatic
                    }
                },
                new Card {
                    ID = 2,
                    IsValid = true,
                    Clozes = secondCardClozes,
                },
            };

            clozes = new List<Cloze>(firstCardClozes.Concat(secondCardClozes));

            deletedCards = new List<Card> {
                 new Card { ID = 11, IsDeleted = true },
                 new Card { ID = 12, IsDeleted = true },
                 new Card { ID = 13, IsDeleted = true },
            };

            draftCards = new List<Card> {
                 new Card { ID = 21, IsValid = false },
                 new Card { ID = 22, IsValid = false },
                 new Card { ID = 23, IsValid = false },
                 new Card { ID = 24, IsValid = false },
            };

            decks = new List<Deck> {
                 new Deck { ID = 1, Cards = validCards, Owner = userName },
                 new Deck { ID = 2, Cards = deletedCards },
                 new Deck { ID = 3, Cards = draftCards },
                 new Deck { ID = 4, Cards = new List<Card>() },
            };

            answers = new List<Answer> {
                 new Answer { ID = 1 },
                 new Answer { ID = 2 },
                 new Answer { ID = 3 },
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
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(clozes.Count(), model.Count());
        }

        [TestMethod()]
        public async Task CardsCardsIndexTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.CardsIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<Card>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(validCards.Count(), model.Count());
        }

        [TestMethod()]
        public async Task CardsDeletedIndexTest()
        {
            // Arrange
            var id = 2;

            // Act
            var result = await sut.DeletedIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<EditCardViewModel>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(deletedCards.Count(), model.Count());
        }

        [TestMethod()]
        public async Task CardsDraftIndexTest()
        {
            // Arrange
            var id = 3;

            // Act
            var result = await sut.DraftIndex(id) as ViewResult;
            var model = result.Model as IEnumerable<EditCardViewModel>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(draftCards.Count(), model.Count());
        }

        [TestMethod()]
        public async Task CardsDetailsTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Details(id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsPreviewClosedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.PreviewClosed(id) as ViewResult;
            var model = result.Model as Card;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task CardsPreviewOpenedGetTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.PreviewOpened(id) as ViewResult;
            var model = result.Model as Card;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task CardsPreviewOpenedPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = 1 };

            // Act
            var result = await sut.PreviewOpened(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRepeatClosedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.RepeatClosed(id) as ViewResult;
            var model = result.Model as Card;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.RepeatOpened(id) as ViewResult;
            var model = result.Model as Card;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedAgainButtonPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = 1 };

            // Act
            var result = await sut.RepeatOpened(card, "againButton", null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedBadButtonPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = 1 };

            // Act
            var result = await sut.RepeatOpened(card, null, "badButton", null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedGoodButtonPostTest()
        {
            // Arrange
            var card = new ViewCardViewModel { ID = 1 };

            // Act
            var result = await sut.RepeatOpened(card, null, null, "goodButton") as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsQuestionTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Question(id) as ViewResult;
            var model = result.Model as AnswerCardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task CardsQuestionPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Question(card) as ViewResult;
            var model = result.Model as AnswerCardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(card.ID, model.ID);
        }

        [TestMethod()]
        public async Task CardsRightTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Right(card) as RedirectToRouteResult;

            // Assert
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
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsTypoTypoButtonPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Typo(card, "TypoButton", null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsTypoWrongButtonPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Typo(card, null, "WrongButton", null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsTypoAltButtonPostTest()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Typo(card, null, null, "AltButton") as RedirectToRouteResult;

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
            var id = 10;
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
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task CardsEditPostTest()
        {
            // Arrange
            var card = new EditCardViewModel { ID = 1 };

            // Act
            var result = await sut.Edit(card) as RedirectToRouteResult;

            // Assert
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
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRestoreTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Restore(id) as ViewResult;
            var model = result.Model as Card;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task CardsRestoreConfirmedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.RestoreConfirmed(id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsDeleteTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Delete(id) as ViewResult;
            var model = result.Model as Card;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task CardsDeleteConfirmedTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.DeleteConfirmed(id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}