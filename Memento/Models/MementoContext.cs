using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Memento.Models;

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
            var userName = user.Identity.Name;
            var items = Decks.Where(item => item.Owner == userName);

            return items;
        }

        public IQueryable<Card> GetUserCards(IPrincipal user)
        {
            var userName = user.Identity.Name;
            var items = Cards.Where(item => item.Deck.Owner == userName);

            return items;
        }

        public IQueryable<Cloze> GetUserClozes(IPrincipal user)
        {
            var userName = user.Identity.Name;
            var items = Clozes.Where(item => item.Card.Deck.Owner == userName);

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
