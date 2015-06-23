using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Memento;
using Memento.Models;

namespace Memento.Controllers
{
    public enum States
    {
        Input,
        Right,
        Wrong,
        Typo,
        Correction,
    }

    public class CardsController : Controller
    {
        private MementoContext db = new MementoContext();

        // GET: Cards
        public async Task<ActionResult> Index(int? DeckID)
        {
            if (DeckID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var deck = await db.Decks.FindAsync(DeckID);

                if (!deck.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    return View(deck.Cards.ToList());
                }
            }
        }

        public async Task<ActionResult> Details(int? id, int? DeckID)
        {
            ViewBag.State = States.Input;

            if (id == null)
            {
                if (DeckID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                else
                {
                    return View(new Card { DeckID = DeckID.Value, ID = -1 });
                }
            }
            else
            {
                var card = await db.Cards.FindAsync(id);

                if (card == null)
                {
                    return HttpNotFound();
                }
                else if (!card.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    return View(card);
                }
            }
        }

        // GET: Cards/Create
        public ActionResult Create()
        {
            ViewBag.DeckID = new SelectList(db.GetUserDecks(User), "ID", "Title");

            var card = new Card();

            return View(card);
        }

        // GET: Cards/Create/5
        public ActionResult Create(int? DeckID)
        {
            if (DeckID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var card = new Card { DeckID = DeckID.Value };

                return View(card);
            }
        }

        // POST: Cards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,DeckID,Text")] Card card)
        {
            if (ModelState.IsValid)
            {
                db.Cards.Add(card);

                await db.SaveChangesAsync();

                return RedirectToAction("Details", "Deck", new { id = card.DeckID });
            }
            else
            {
                return View(card);
            }
        }

        // GET: Cards/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var card = await db.Cards.FindAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(card);
            }
        }

        // POST: Cards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Text")] Card card)
        {
            if (ModelState.IsValid)
            {
                var dbCard = await db.Cards.FindAsync(card.ID);

                if (!dbCard.IsAuthorized(User))
                {
                    return new HttpUnauthorizedResult();
                }
                else
                {
                    dbCard.Text = card.Text;

                    await db.SaveChangesAsync();

                    return RedirectToAction("Details", "Decks", new { id = dbCard.DeckID });
                }
            }
            else
            {
                return View(card);
            }
        }

        // GET: Cards/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var card = await db.Cards.FindAsync(id);

            if (card == null)
            {
                return HttpNotFound();
            }
            else if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                return View(card);
            }
        }

        // POST: Cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var card = await db.Cards.FindAsync(id);

            if (!card.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            db.Cards.Remove(card);

            await db.SaveChangesAsync();

            return RedirectToAction("Details", "Deck", new { id = card.DeckID });
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
