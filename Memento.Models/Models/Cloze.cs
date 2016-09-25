using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.Models
{
    public class Cloze : ICloze
    {
        public Cloze()
        {
        }

        public Cloze(int cardID, string label)
        {
            CardID = cardID;
            Label = label;
        }

        public int ID { get; set; }

        public int CardID { get; set; }

        public virtual Card Card { get; set; }

        public virtual ICollection<UserRepetition> UserRepetitions { get; set; }

        public ICard GetCard()
        {
            return Card;
        }

        public IUserRepetition GetUserRepetition(string username)
        {
            return UserRepetitions.Single(x => x.UserName == username);
        }

        public IEnumerable<string> GetUsers() =>
            UserRepetitions.Select(x => x.UserName).Distinct();

        public string Label { get; set; }
    }
}
