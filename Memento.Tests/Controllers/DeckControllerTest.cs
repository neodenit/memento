using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Memento.Web.Controllers;
using Memento.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Memento.DomainModel;

namespace Memento.Tests.Controllers
{
    [TestClass]
    public class DeckControllerTest
    {
        [TestMethod]
        public void Create()
        {
            var controller = new DecksController();

            var result = controller.Create() as ViewResult;

            Assert.IsNotNull(result);
            var model = result.Model as Deck;
            Assert.IsNotNull(model);
            Assert.AreEqual(Settings.Default.StartDelay, model.StartDelay);
            Assert.AreEqual(Settings.Default.Coeff, model.Coeff, double.Epsilon);
        }
    }
}
