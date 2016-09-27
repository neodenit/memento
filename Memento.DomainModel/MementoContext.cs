﻿using Memento.Models.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Memento.Models.Models
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
