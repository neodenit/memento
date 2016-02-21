using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memento.DomainModel.Repository;
using Memento.Interfaces;
using Moq;
using System.Web.Mvc;
using Memento.DomainModel.Models;
using Memento.Core;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Memento.Tests.TestDbAsync;

namespace Memento.Web.Controllers.Tests
{
    [TestClass()]
    public class DecksControllerTests
    {
        private Mock<IMementoRepository> mockRepository;
        private Mock<IConverter> mockConverter;
        private Mock<IValidator> mockValidator;
        private Mock<IScheduler> mockScheduler;
        private DecksController sut;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new Mock<IMementoRepository>();
            mockConverter = new Mock<IConverter>();
            mockValidator = new Mock<IValidator>();
            mockScheduler = new Mock<IScheduler>();

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(item => item.HttpContext.User.Identity.Name).Returns("user@server.com");

            sut = new DecksController(mockRepository.Object, mockConverter.Object, mockValidator.Object, mockScheduler.Object)
            {
                ControllerContext = mockContext.Object
            };

            AddDbSetMocking();
        }

        private void AddDbSetMocking()
        {
            var data = new List<Deck> {
                 new Deck { ID = 1, Title = "Deck 1", Cards = new[] { new Card(), new Card() } },
                 new Deck { ID = 2, Title = "Deck 2" },
                 new Deck { ID = 3, Title = "Deck 3" },
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
                .Setup(x => x.GetUserDecks(It.IsAny<string>()))
                .Returns(mockDbSet.Object);

            mockRepository
                .Setup(x => x.FindDeckAsync(It.IsAny<int>()))
                .Returns<int>(x => Task.FromResult(data.FirstOrDefault(d => d.ID == x)));//todo
        }

        [TestMethod()]
        public async Task DecksIndexTest()
        {
            // Arrange

            // Act
            var result = await sut.Index() as ViewResult;
            var model = result.Model as List<Deck>;

            // Assert
            Assert.IsNotNull(model);
        }

        [TestMethod()]
        public async Task DecksDetailsGetTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await sut.Details(id) as ViewResult;
            var model = result.Model as Deck;

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            mockRepository.Verify(x => x.GetAnswersForDeck(id), Times.Once);
            Assert.IsNotNull(model);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task DecksDetailsPostTest()
        {
            // Arrange
            var deck = new Deck { ID = 1 };

            // Act
            var result = await sut.Details(deck) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void DecksCreateGetTest()
        {
            // Arrange

            // Act
            var result = sut.Create() as ViewResult;
            var model = result.Model as Deck;

            // Assert
            Assert.AreEqual(Settings.Default.StartDelay, model.StartDelay);
            Assert.AreEqual(Settings.Default.Coeff, model.Coeff, double.Epsilon);
        }

        [TestMethod()]
        public async Task DecksCreatePostTest()
        {
            // Arrange
            var deck = new Deck { ID = 4, Title = "Deck 4" };

            // Act
            var result = await sut.Create(deck) as RedirectToRouteResult;

            // Assert
            mockRepository.Verify(x => x.AddDeck(deck), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
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
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task DecksEditPostTest()
        {
            // Arrange
            var deck = new Deck { ID = 1 };

            // Act
            var result = await sut.Edit(deck) as RedirectToRouteResult;

            // Assert
            mockRepository.Verify(x => x.FindDeckAsync(deck.ID), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
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
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
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
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            mockRepository.Verify(x => x.RemoveDeck(It.IsAny<Deck>()), Times.Once);
            mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void DecksImportGetTest()
        {
            // Arrange
            var id = 1;

            // Act
            var result = sut.Import(id) as ViewResult;
            var model = result.Model as Deck;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, model.ID);
        }

        [TestMethod()]
        public async Task DecksImportPostTest()
        {
            // Arrange
            var deck = new Deck { ID = 1 };

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
            mockRepository.Verify(x => x.FindDeckAsync(id), Times.Once);
            Assert.IsNotNull(result);
        }
    }
}