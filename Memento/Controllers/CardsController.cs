﻿using System;
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

        public ActionResult DetailsEmpty(int? DeckID)
        {
            if (DeckID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                return View("Details", new Card { DeckID = DeckID.Value, ID = -1 });
            }
        }

        public async Task<ActionResult> Details(int id)
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

            var cloze = card.GetNextCloze();

            if (cloze.IsNew)
            {
                return RedirectToAction("PreviewClosed", new { id = card.ID });
            }
            else
            {
                return RedirectToAction("Question", new { id = card.ID });
            }
        }

        public async Task<ActionResult> PreviewClosed(int id)
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
            
            var cloze = card.GetNextCloze();

            var question = Converter.GetQuestion(card.Text, cloze.Label);

            card.Text = question;

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewClozed([Bind(Include = "ID")]Card card)
        {
            var dbCard = await db.Cards.FindAsync(card.ID);

            if (dbCard == null)
            {
                return HttpNotFound();
            }
            else if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }

            return RedirectToAction("PreviewOpened", new { id = dbCard.ID });
        }

        public async Task<ActionResult> PreviewOpened(int id)
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

            var cloze = card.GetNextCloze();

            var question = Converter.GetAnswer(card.Text, cloze.Label);

            card.Text = question;

            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PreviewOpened([Bind(Include = "ID")]Card card)
        {
            var dbCard = await db.Cards.FindAsync(card.ID);

            if (dbCard == null)
            {
                return HttpNotFound();
            }
            else if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var deck = dbCard.Deck;

                var clozes = deck.GetClozes();

                Scheduler.PromoteCard(deck, clozes, Scheduler.Delays.Same);

                await db.SaveChangesAsync();

                var nextCard = deck.GetNextCard();

                return RedirectToAction("Details", "Cards", new { id = nextCard.ID });
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

                card.Text = Converter.GetQuestion(card.Text, cloze.Label);

                return View(card);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Question([Bind(Include = "ID, Answer")]Card card)
        {
            var dbCard = await db.Cards.FindAsync(card.ID);

            if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var cloze = dbCard.GetNextCloze();

                var answer = Converter.GetAnswerValue(dbCard.Text, cloze.Label);

                if (card.Answer == answer)
                {
                    dbCard.Text = Converter.GetAnswer(dbCard.Text, cloze.Label);

                    return View("Right", dbCard);
                }
                else
                {
                    ViewBag.Answer = card.Answer;

                    dbCard.Text = Converter.GetAnswer(dbCard.Text, cloze.Label);

                    return View("Wrong", dbCard);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Right([Bind(Include = "ID")]Card card)
        {
            var dbCard = await db.Cards.FindAsync(card.ID);

            if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else
            {
                var deck = dbCard.Deck;
                var clozes = deck.GetClozes();

                Scheduler.PromoteCard(deck, clozes, Scheduler.Delays.Next);

                await db.SaveChangesAsync();

                var nextCard = deck.GetNextCard();

                return RedirectToAction("Question", new { id = nextCard.ID });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wrong([Bind(Include = "ID")]Card card, string NextButton, string AltButton)
        {
            var dbCard = await db.Cards.FindAsync(card.ID);

            if (!dbCard.IsAuthorized(User))
            {
                return new HttpUnauthorizedResult();
            }
            else if (NextButton != null)
            {
                var deck = dbCard.Deck;
                var clozes = deck.GetClozes();

                Scheduler.PromoteCard(deck, clozes, Scheduler.Delays.Previous);

                await db.SaveChangesAsync();

                var nextCard = deck.GetNextCard();

                return RedirectToAction("Question", new { id = nextCard.ID });
            }
            else if (AltButton != null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                
                var text = card.Text;

                var clozeNames = Converter.GetClozeNames(text);

                if (!clozeNames.Any() || !clozeNames.All(clozeName => Validator.ValidateFull(text, clozeName)))
                {
                    return View(card);
                }
                else
                {
                    db.Cards.Add(card);

                    await db.SaveChangesAsync();

                    foreach (var clozeName in clozeNames)
                    {

                        var cloze = new Cloze(card.ID, clozeName);

                        Scheduler.PrepareForAdding(deck, db.Clozes, cloze);

                        db.Clozes.Add(cloze);
                    }

                    await db.SaveChangesAsync();

                    return RedirectToAction("Details", "Deck", new { id = card.DeckID });
                }
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
