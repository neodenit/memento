using Memento.Common;
using Memento.Interfaces;
using Memento.Models.Models;
using Memento.Models.ViewModels;
using Memento.Tests.TestDbAsync;
using Memento.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Memento.Tests.Controllers
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

        private Mock<IFactory> mockFactory;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();

            mockDecksService = new Mock<IDecksService>();
            mockCardsService = new Mock<ICardsService>();
            mockStatisticsService = new Mock<IStatisticsService>();
            mockExportImportService = new Mock<IExportImportService>();
            mockSchedulerService = new Mock<ISchedulerService>();

            mockFactory = new Mock<IFactory>();

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(item => item.HttpContext.User.Identity.Name).Returns("user@server.com");

            sut = new DecksController(mockDecksService.Object, mockCardsService.Object, mockStatisticsService.Object, mockExportImportService.Object, mockSchedulerService.Object, mockFactory.Object)
            {
                ControllerContext = mockContext.Object
            };

            mockStatisticsService.Setup(x => x.GetAnswersAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync(Enumerable.Empty<IAnswer>());

            mockDecksService.Setup(x => x.GetDeckWithStatViewModel(It.IsAny<int>(), It.IsAny<IStatistics>())).ReturnsAsync(new DeckWithStatViewModel());

            mockDecksService.Setup(x => x.FindDeckAsync(It.IsAny<int>())).Returns<int>(async x => await Task.FromResult(
                new Deck
                {
                    ID = x,
                    Title = "title",
                    Cards = new List<Card>
                    {
                        new Card
                        {
                            IsValid = true,
                            Clozes = new List<Cloze>
                            {
                                new Cloze()
                            }
                        }
                    }
                }));

            mockExportImportService.Setup(x => x.Export(It.IsAny<int>())).ReturnsAsync(string.Empty);

            mockFactory.Setup(x => x.CreateDeck(It.IsAny<double>(), It.IsAny<int>())).Returns<double, int>((x, y) => new Deck { Coeff = x, StartDelay = y });

            mockFactory.Setup(x => x.CreateDeck(It.IsAny<int>())).Returns<int>((x) => new Deck { ID = x });
        }

        private void AddDbSetMocking()
        {
            var data = new List<Deck> {
                 new Deck { ID = 1, Cards = new[] { new Card() } },
                 new Deck { ID = 2, },
                 new Deck { ID = 3, },
            };

            var dataQuery = data.AsQueryable();

            var mockDbSet = new Mock<DbSet<Deck>>();

            mockDbSet.As<IDbAsyncEnumerable<Deck>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Deck>(dataQuery.GetEnumerator()));

            mockDbSet.As<IQueryable<Deck>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<Deck>(dataQuery.Provider));

            mockDbSet.As<IQueryable<Deck>>().Setup(m => m.Expression).Returns(dataQuery.Expression);
            mockDbSet.As<IQueryable<Deck>>().Setup(m => m.ElementType).Returns(dataQuery.ElementType);
            mockDbSet.As<IQueryable<Deck>>().Setup(m => m.GetEnumerator()).Returns(dataQuery.GetEnumerator());

            mockRepository
                .Setup(x => x.GetUserDecksAsync(It.IsAny<string>()))
                .Returns(async () => await mockDbSet.Object.ToListAsync<IDeck>());

            mockRepository
                .Setup(x => x.FindDeckAsync(It.IsAny<int>()))
                .Returns<int>(x => Task.FromResult(data.FirstOrDefault(d => d.ID == x) as IDeck));
        }

        [TestMethod()]
        public async Task DecksIndexTest()
        {
            // Arrange

            // Act
            var result = await sut.Index() as ViewResult;
            var model = result.Model as IEnumerable<Deck>;

            // Assert
            mockDecksService.Verify(x => x.GetDecksAsync(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task DecksDetailsGetTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Details(id) as ViewResult;
            var model = result.Model as DeckWithStatViewModel;

            // Assert
            mockStatisticsService.Verify(x => x.GetAnswersAsync(id, It.IsAny<DateTime>()), Times.Once);
            mockStatisticsService.Verify(x => x.GetStatistics(It.IsAny<IEnumerable<IAnswer>>()), Times.Once);
            mockDecksService.Verify(x => x.GetDeckWithStatViewModel(id, It.IsAny<IStatistics>()));

            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task DecksDetailsPostTest()
        {
            // Arrange
            var deck = new Deck { ID = 1 };

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
            var userName = "user@server.com";

            // Act
            var result = await sut.Create(deck) as RedirectToRouteResult;

            // Assert
            mockDecksService.Verify(x => x.CreateDeck(It.IsAny<Deck>(), userName), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DecksEditGetTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Edit(id) as ViewResult;
            var model = result.Model as Deck;

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
            var deck = new Deck
            {
                ID = 1,
                Title = "Title",
                StartDelay = 8,
                Coeff = 2.0,
            };

            // Act
            var result = await sut.Edit(deck) as RedirectToRouteResult;

            // Assert
            mockDecksService.Verify(x => x.UpdateDeck(deck.ID, deck.Title, deck.StartDelay, deck.Coeff), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DecksDeleteTest()
        {
            // Arrange
            var id = 1;

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
            var id = 1;

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
            var id = 1;

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
            var deck = new ImportViewModel { DeckID = 1 };

            // Act
            var result = await sut.Import(deck, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task DecksExportTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Export(id) as FileContentResult;

            // Assert
            mockExportImportService.Verify(x => x.Export(id), Times.Once);
            mockDecksService.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
        }
    }
}