using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Memento.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Memento.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private HomeController sut;

        [TestInitialize]
        public void Setup()
        {
            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(item => item.HttpContext.User.Identity.IsAuthenticated).Returns(true);

            sut = new HomeController { ControllerContext = mockContext.Object };
        }

        [TestMethod]
        public void HomeIndexTest()
        {
            // Arrange

            // Act
            var result = sut.Index() as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void HomeAboutTest()
        {
            // Arrange

            // Act
            var result = sut.About() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void HomeContactTest()
        {
            // Arrange

            // Act
            var result = sut.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
