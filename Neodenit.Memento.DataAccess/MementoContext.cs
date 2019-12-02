using Memento.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Memento.DataAccess
{
    public class MementoContext : DbContext
    {
        public MementoContext(DbContextOptions<MementoContext> options) : base(options) { }

        public DbSet<Deck> Decks { get; set; }

        public DbSet<Card> Cards { get; set; }

        public DbSet<Cloze> Clozes { get; set; }

        public DbSet<UserRepetition> Repetitions { get; set; }

        public DbSet<Answer> Answers { get; set; }
    }
}
