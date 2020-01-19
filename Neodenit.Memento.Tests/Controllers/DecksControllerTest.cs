using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neodenit.Memento.Common;
using Neodenit.Memento.Common.DataModels;
using Neodenit.Memento.Common.ViewModels;
using Neodenit.Memento.DataAccess.API;
using Neodenit.Memento.Services.API;
using Neodenit.Memento.Web.Controllers;

namespace Neodenit.Memento.Tests.Controllers
{
    [TestClass()]
    public class DecksControllerTest
    {
        private DecksController sut;
        private Mock<IMementoRepository> mockRepository;
        private Mock<IDecksService> mockDecksService;
        private Mock<ICardsService> mockCardsService;
        private Mock<IStatisticsService> mockStatisticsService;
        private Mock<IExportImportService> mockExportImportService;
        private Mock<ISchedulerService> mockSchedulerService;

        private readonly string userName = "user@server.com";
        private Guid deckId = new Guid("00000000-0000-0000-0000-000000000001");

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();

            mockDecksService = new Mock<IDecksService>();
            mockCardsService = new Mock<ICardsService>();
            mockStatisticsService = new Mock<IStatisticsService>();
            mockExportImportService = new Mock<IExportImportService>();
            mockSchedulerService = new Mock<ISchedulerService>();

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(item => item.HttpContext.User.Identity.Name).Returns("user@server.com");

            sut = new DecksController(mockDecksService.Object, mockCardsService.Object, mockStatisticsService.Object, mockExportImportService.Object, mockSchedulerService.Object)
            {
                ControllerContext = mockContext.Object
            };

            mockDecksService.Setup(x => x.GetDeckWithStatViewModel(It.IsAny<Guid>(), It.IsAny<StatisticsViewModel>(), It.IsAny<string>())).ReturnsAsync(new DeckWithStatViewModel());

            var deckViewModel = new DeckViewModel();

            var deck = new Deck
            {
                ID = deckId,
                Title = "title",
                Cards = new List<Card>
                    {
                        new Card
                        {
                            IsValid = true,
                            Clozes = new List<Cloze>
                            {
                                new Cloze
                                {
                                    UserRepetitions = new List<UserRepetition>
                                    {
                                        new UserRepetition
                                        {
                                            UserName = userName
                                        }
                                    }
                                }
                            }
                        }
                    }
            };

            mockDecksService.Setup(x => x.FindDeckAsync(It.IsAny<Guid>())).Returns<Guid>(async x =>
                await Task.FromResult(new DeckViewModel { ID = x }));

            mockExportImportService.Setup(x => x.Export(It.IsAny<Guid>())).ReturnsAsync(string.Empty);
        }

        private void AddDbSetMocking()
        {
            var data = new List<Deck> {
                 new Deck { ID = new Guid("00000000-0000-0000-0000-000000000001"), Cards = new[] { new Card() } },
                 new Deck { ID = new Guid("00000000-0000-0000-0000-000000000002"), },
                 new Deck { ID = new Guid("00000000-0000-0000-0000-000000000003"), },
            };

            var dataQuery = data.AsQueryable();

            mockRepository
                .Setup(x => x.GetUserDecksAsync(It.IsAny<string>()))
                .Returns(async () => await Task.FromResult(data));

            mockRepository
                .Setup(x => x.FindDeckAsync(It.IsAny<Guid>()))
                .Returns<Guid>(x => Task.FromResult(data.FirstOrDefault(d => d.ID == x) as Deck));
        }

        [TestMethod()]
        public async Task DecksIndexTest()
        {
            // Arrange

            // Act
            var result = await sut.Index() as ViewResult;
            var model = result.Model as DecksViewModel;

            // Assert
            mockDecksService.Verify(x => x.GetDecksAsync(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task DecksDetailsGetTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.Details(id) as ViewResult;
            var model = result.Model as DeckWithStatViewModel;

            // Assert
            mockStatisticsService.Verify(x => x.GetStatisticsAsync(id, It.IsAny<DateTime>()), Times.Once);
            mockDecksService.Verify(x => x.GetDeckWithStatViewModel(id, It.IsAny<StatisticsViewModel>(), It.IsAny<string>()));

            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task DecksDetailsPostTest()
        {
            // Arrange
            var deck = new DeckViewModel { ID = deckId };

            // Act
            var result = await sut.Details(deck) as RedirectToRouteResult;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(deck.ID), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void DecksCreateGetTest()
        {
            // Arrange

            // Act
            var result = sut.Create() as ViewResult;
            var model = result.Model as DeckViewModel;

            // Assert
            Assert.IsNotNull(model);

            if (Settings.Default.EnableTwoStepsConfig)
            {
                Assert.AreEqual(Settings.Default.FirstDelay, model.FirstDelay);
                Assert.AreEqual(Settings.Default.SecondDelay, model.SecondDelay);
            }
            else
            {
                Assert.AreEqual(Settings.Default.StartDelay, model.StartDelay);
                Assert.AreEqual(Settings.Default.Coeff, model.Coeff, double.Epsilon);
            }
        }

        [TestMethod()]
        public async Task DecksCreatePostTest()
        {
            // Arrange
            var deck = new DeckViewModel();

            // Act
            var result = await sut.Create(deck) as RedirectToRouteResult;

            // Assert
            mockDecksService.Verify(x => x.CreateDeck(It.IsAny<DeckViewModel>(), userName), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DecksEditGetTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.Edit(id) as ViewResult;
            var model = result.Model as DeckViewModel;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task DecksEditPostTest()
        {
            // Arrange
            var deck = new DeckViewModel
            {
                ID = deckId,
                Title = "Title",
                StartDelay = 8,
                Coeff = 2.0,
                PreviewAnswer = true,
            };

            // Act
            var result = await sut.Edit(deck) as RedirectToRouteResult;

            // Assert
            if (Settings.Default.EnableTwoStepsConfig)
            {
                mockDecksService.Verify(x => x.UpdateDeck(deck.ID, deck.Title, It.IsAny<int>(), It.IsAny<double>(), deck.PreviewAnswer), Times.Once);
            }
            else
            {
                mockDecksService.Verify(x => x.UpdateDeck(deck.ID, deck.Title, deck.StartDelay, deck.Coeff, deck.PreviewAnswer), Times.Once);
            }

            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DecksDeleteTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.Delete(id) as ViewResult;
            var model = result.Model as Deck;

            // Assert
            mockDecksService.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task DecksDeleteConfirmedTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.DeleteConfirmed(id) as RedirectToRouteResult;

            // Assert
            mockDecksService.Verify(x => x.DeleteDeck(id), Times.Once);

            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void DecksImportGetTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = sut.Import(id) as ViewResult;
            var model = result.Model as ImportViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.DeckID);
            Assert.IsTrue(model.IsShuffled);
        }

        [TestMethod()]
        public async Task DecksImportPostTest()
        {
            // Arrange
            var deck = new ImportViewModel { DeckID = deckId };

            // Act
            var result = await sut.Import(deck, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DecksExportTest()
        {
            // Arrange
            var id = deckId;

            // Act
            var result = await sut.Export(id) as FileContentResult;

            // Assert
            mockExportImportService.Verify(x => x.Export(id), Times.Once);
            mockDecksService.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
        }
    }
}