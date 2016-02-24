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
    public class CardsControllerTestWithFakeRepo
    {
        private CardsController sut;

        private FakeRepository fakeRepository;

        private Mock<IConverter> mockConverter;
        private Mock<IValidator> mockValidator;
        private Mock<IScheduler> mockScheduler;
        private Mock<IEvaluator> mockEvaluator;

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
            AddFakeRepository();
            mockConverter = new Mock<IConverter>();
            mockValidator = new Mock<IValidator>();
            mockScheduler = new Mock<IScheduler>();
            mockEvaluator = new Mock<IEvaluator>();

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(item => item.HttpContext.User.Identity.Name).Returns(userName);

            sut = new CardsController(fakeRepository, mockConverter.Object, mockValidator.Object, mockScheduler.Object, mockEvaluator.Object)
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

            fakeRepository = new FakeRepository(decks, validCards, clozes, answers);
        }

        [TestMethod()]
        public async Task CardsClozesIndexTestWithFakeRepo()
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
        public async Task CardsCardsIndexTestWithFakeRepo()
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
        public async Task CardsDeletedIndexTestWithFakeRepo()
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
        public async Task CardsDraftIndexTestWithFakeRepo()
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
        public void CardsDetailsEmptyTestWithFakeRepo()
        {
            // Arrange
            var id = 1;

            // Act
            var result = sut.DetailsEmpty(id) as ViewResult;
            var model = result.Model as Card;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.DeckID);
            Assert.AreEqual(-1, model.ID);
        }

        [TestMethod()]
        public async Task CardsDetailsTestWithFakeRepo()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Details(id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsPreviewClosedTestWithFakeRepo()
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
        public async Task CardsPreviewOpenedGetTestWithFakeRepo()
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
        public async Task CardsPreviewOpenedPostTestWithFakeRepo()
        {
            // Arrange
            var card = new Card { ID = 1 };

            // Act
            var result = await sut.PreviewOpened(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRepeatClosedTestWithFakeRepo()
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
        public async Task CardsRepeatOpenedTestWithFakeRepo()
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
        public async Task CardsRepeatOpenedAgainButtonPostTestWithFakeRepo()
        {
            // Arrange
            var card = new Card { ID = 1 };

            // Act
            var result = await sut.RepeatOpened(card, "againButton", null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedBadButtonPostTestWithFakeRepo()
        {
            // Arrange
            var card = new Card { ID = 1 };

            // Act
            var result = await sut.RepeatOpened(card, null, "badButton", null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRepeatOpenedGoodButtonPostTestWithFakeRepo()
        {
            // Arrange
            var card = new Card { ID = 1 };

            // Act
            var result = await sut.RepeatOpened(card, null, null, "goodButton") as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsQuestionTestWithFakeRepo()
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
        public async Task CardsQuestionPostTestWithFakeRepo()
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
        public async Task CardsRightTestWithFakeRepo()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Right(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsWrongNextButtonPostTestWithFakeRepo()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Wrong(card, "NextButton", null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsWrongAltButtonPostTestWithFakeRepo()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Wrong(card, null, "AltButton") as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsTypoTypoButtonPostTestWithFakeRepo()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Typo(card, "TypoButton", null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsTypoWrongButtonPostTestWithFakeRepo()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Typo(card, null, "WrongButton", null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsTypoAltButtonPostTestWithFakeRepo()
        {
            // Arrange
            var card = new AnswerCardViewModel { ID = 1 };

            // Act
            var result = await sut.Typo(card, null, null, "AltButton") as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsCreateTestWithFakeRepo()
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
        public async Task CardsCreateNullDeckIDTestWithFakeRepo()
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
        public async Task CardsCreatePostTestWithFakeRepo()
        {
            // Arrange
            var id = 10;
            var card = new EditCardViewModel { ID = id };

            // Act
            var result = await sut.Create(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(fakeRepository.FindCard(id));
        }

        [TestMethod()]
        public async Task CardsEditTestWithFakeRepo()
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
        public async Task CardsEditPostTestWithFakeRepo()
        {
            // Arrange
            var card = new Card { ID = 1 };

            // Act
            var result = await sut.Edit(card) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsShuffleNewCardsTestWithFakeRepo()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.ShuffleNewCards(id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsShuffleNewTestWithFakeRepo()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.ShuffleNew(id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsRestoreTestWithFakeRepo()
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
        public async Task CardsRestoreConfirmedTestWithFakeRepo()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.RestoreConfirmed(id) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task CardsDeleteTestWithFakeRepo()
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
        public async Task CardsDeleteConfirmedTestWithFakeRepo()
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