using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Memento.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Memento";
            
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Decks");
            }
            else
            {
                return View();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Memento";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Memento";

            return View();
        }
    }
}