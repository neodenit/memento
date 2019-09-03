using System;
using System.Collections.Generic;
using System.Linq;
using Memento.Interfaces;

namespace Memento.Models.Models
{
    public class Cloze : ICloze
    {
        public Cloze()
        {
            UserRepetitions = new List<UserRepetition>();
        }

        public Cloze(Guid cardID, string label) : this()
        {
            CardID = cardID;
            Label = label;
        }

        public int ID { get; set; }

        public Guid CardID { get; set; }

        public virtual Card Card { get; set; }

        public virtual ICollection<UserRepetition> UserRepetitions { get; set; }

        public ICard GetCard() => Card;

        public IUserRepetition GetUserRepetition(string username) =>
            UserRepetitions.SingleOrDefault(x => x.UserName == username);

        public IEnumerable<string> GetUsers() =>
            UserRepetitions.Select(x => x.UserName).Distinct();

        public string Label { get; set; }
    }
}
