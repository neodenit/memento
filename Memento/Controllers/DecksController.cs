using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ionic.Zip;
using Memento;
using Memento.Models;
using Memento.SRS;
using Microsoft.AspNet.Identity;

namespace Memento.Controllers
{
    [Authorize]
#if !DEBUG
[RequireHttps]
#endif
    public class DecksController : Controller
    {
        private MementoContext db = new MementoContext();

        // GET: Decks
        public async Task<ActionResult> Index()
        {
            var items = db.GetUserDecks(User);

            return View(await items.ToListAsync());
        }

        public ActionResult Export()
        {
            var decks = db.GetUserDecks(User);

            return new JsonResult { Data = decks, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // GET: Decks/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await db.Decks.FindAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(deck);
            }
        }

        // POST: Decks/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details([Bind(Include = "ID")]Deck deck)
        {
            var dbDeck = await db.Decks.FindAsync(deck.ID);

            if (dbDeck == null)
            {
                return HttpNotFound();
            }
            else if (!dbDeck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else if (dbDeck.Cards.Any())
            {
                var card = dbDeck.Cards.GetMinElement(item => item.Clozes.Min(c => c.Position));

                return RedirectToAction("Details", "Cards", new { id = card.ID });
            }
            else
            {
                return RedirectToAction("Details", "Cards", new { DeckID = dbDeck.ID });
            }
        }

        // GET: Decks/Create
        public ActionResult Create()
        {
            var deck = new Deck { Coeff = Settings.Default.Coeff, StartDelay = Settings.Default.StartDelay };

            return View(deck);
        }

        // POST: Decks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Title,ControlMode,DelayMode,StartDelay,Coeff")] Deck deck)
        {
            if (deck.ControlMode == ControlModes.Automatic && deck.DelayMode == DelayModes.Combined)
            {
                throw new Exception();
            }
            else if (ModelState.IsValid)
            {

                deck.OwnerID = User.Identity.GetUserId();

                db.Decks.Add(deck);

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        // GET: Decks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await db.Decks.FindAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(deck);
            }
        }

        // POST: Decks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Title,StartDelay,Coeff")] Deck deck)
        {
            if (ModelState.IsValid)
            {
                var dbDeck = await db.Decks.FindAsync(deck.ID);

                if (deck == null)
                {
                    return HttpNotFound();
                }
                else if (!dbDeck.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }

                dbDeck.Title = deck.Title;
                dbDeck.StartDelay = deck.StartDelay;
                dbDeck.Coeff = deck.Coeff;

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View(deck);
            }
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var deck = await db.Decks.FindAsync(id);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(deck);
            }
        }

        // POST: Decks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var deck = await db.Decks.FindAsync(id);

            if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            db.Decks.Remove(deck);

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public ActionResult Upload(int DeckID)
        {
            var deckWithID = new Deck { ID = DeckID };

            return View(deckWithID);
        }

        [HttpPost]
        public async Task<ActionResult> Upload(Deck deckWithID, HttpPostedFileBase file)
        {
            var deck = await db.Decks.FindAsync(deckWithID.ID);

            if (deck == null)
            {
                return HttpNotFound();
            }
            else if (!deck.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            if (file != null && file.ContentLength > 0)
            {
                var text = new StreamReader(file.InputStream).ReadToEnd();

                var cards = DeckConverter.GetCardsFromDeck(text);

                db.Cards.Where(item => item.DeckID == deck.ID).ToList().ForEach(item => db.Cards.Remove(item));

                foreach (var card in cards)
                {
                    var newCard = new Card
                    {
                        DeckID = deckWithID.ID,
                        Text = card,
                    };

                    db.Cards.Add(newCard);

                    await db.SaveChangesAsync();

                    var clozeNames = DeckConverter.GetClozeNames(card);

                    foreach (var clozeName in clozeNames)
                    {
                        var newCloze = new Cloze(newCard.ID, clozeName);

                        Scheduler.PrepareForAdding(deck, db.Clozes, newCloze);

                        db.Clozes.Add(newCloze);

                        await db.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
