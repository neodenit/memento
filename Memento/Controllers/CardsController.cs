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
using Memento.SRS;

namespace Memento.Controllers
{
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

        public ActionResult Details(int? DeckID)
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

        public async Task<ActionResult> Question(int? id)
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
                var cloze = card.GetNextCloze();

                card.Text = DeckConverter.GetQuestion(card.Text, cloze.Label);

                return View(card);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Question([Bind(Include = "ID, Answer")]Card card, string NextButton, string AltButton)
        {
            var dbCard = await db.Cards.FindAsync(card.ID);

            if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var cloze = dbCard.GetNextCloze();

                var answer = DeckConverter.GetAnswerValue(dbCard.Text, cloze.Label);

                if (card.Answer == answer)
                {
                    dbCard.Text = DeckConverter.GetAnswer(dbCard.Text, cloze.Label);

                    return View("Right", dbCard);
                }
                else
                {
                    ViewBag.Answer = card.Answer;

                    dbCard.Text = DeckConverter.GetAnswer(dbCard.Text, cloze.Label);

                    return View("Wrong", dbCard);
                }
            }
        }

        // GET: Cards/Create
        public ActionResult Create(int? DeckID)
        {
            if (DeckID == null)
            {
                ViewBag.DeckID = new SelectList(db.GetUserDecks(User), "ID", "Title");

                var card = new Card();

                return View(card);
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
                var deck = await db.Decks.FindAsync(card.DeckID);

                db.Cards.Add(card);

                await db.SaveChangesAsync();

                var clozeNames = DeckConverter.GetClozeNames(card.Text);

                foreach (var clozeName in clozeNames)
                {
                    var cloze = new Cloze(card.ID, clozeName);

                    Scheduler.PrepareForAdding(deck, db.Clozes, cloze);

                    db.Clozes.Add(cloze);
                }

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
