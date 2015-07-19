using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Memento.Models;
using Microsoft.AspNet.Identity;

namespace Memento.Models
{
    public class MementoContext : DbContext
    {
        public MementoContext()
            : base("name=MementoContext")
        {
        }

        public DbSet<Deck> Decks { get; set; }

        public DbSet<Card> Cards { get; set; }

        public DbSet<Cloze> Clozes { get; set; }

        public DbSet<Answer> Answers { get; set; }

        public void DeleteUserDecks(IPrincipal user)
        {
            var items = GetUserDecks(user);
            Decks.RemoveRange(items);
            SaveChanges();
        }

        public void DeleteUserCards(IPrincipal user)
        {
            var items = GetUserCards(user);
            Cards.RemoveRange(items);
            SaveChanges();
        }

        public void DeleteUserClozes(IPrincipal user)
        {
            var items = GetUserClozes(user);
            Clozes.RemoveRange(items);
            SaveChanges();
        }

        public IQueryable<Deck> GetUserDecks(IPrincipal user)
        {
            var userID = user.Identity.GetUserId();
            var items = Decks.Where(item => item.OwnerID == userID);

            return items;
        }

        public IQueryable<Card> GetUserCards(IPrincipal user)
        {
            var userID = user.Identity.GetUserId();
            var items = Cards.Where(item => item.Deck.OwnerID == userID);

            return items;
        }

        public IQueryable<Cloze> GetUserClozes(IPrincipal user)
        {
            var userID = user.Identity.GetUserId();
            var items = Clozes.Where(item => item.Card.Deck.OwnerID == userID);

            return items;
        }

        public class ExportData
        {
            private readonly IEnumerable<Deck> decks;
            private readonly IEnumerable<Card> cards;
            private readonly IEnumerable<Cloze> clozes;
        }
    }
}
