using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Neodenit.Memento.Common;
using Newtonsoft.Json;

namespace Neodenit.Memento.DataAccess.API.DataModels
{
    [Serializable]
    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public Guid ReadingCardId { get; set; }

        public Guid DeckID { get; set; }

        [JsonIgnore]
        public virtual Deck Deck { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public string Comment { get; set; }

        public virtual ICollection<Cloze> Clozes { get; set; } = new List<Cloze>();

        public Cloze GetNextCloze(string username) =>
            Clozes.GetMinElement(c => c.GetUserRepetition(username).Position);

        public void AddCloze(Cloze cloze) =>
            Clozes.Add(cloze as Cloze);

        public IEnumerable<string> GetUsers() =>
            Clozes.SelectMany(x => x.GetUsers()).Distinct();

        public bool IsValid { get; set; }

        public bool IsDeleted { get; set; }
    }
}
