using System.Data.Entity;
using Memento.Models.Models;

namespace Memento.DataAccess
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

        public DbSet<UserRepetition> Repetitions { get; set; }

        public DbSet<Answer> Answers { get; set; }
    }
}
