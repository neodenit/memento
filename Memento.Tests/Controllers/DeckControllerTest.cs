﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Memento;
using Memento.Controllers;
using Memento.Models;
using Memento.SRS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Memento.Tests.Controllers
{
    [TestClass]
    public class DeckControllerTest
    {
        [TestMethod]
        public void Index()
        {
            var controller = new DecksController();

            var result = controller.Index().Result as ViewResult;

            Assert.IsNotNull(result);
        }

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
