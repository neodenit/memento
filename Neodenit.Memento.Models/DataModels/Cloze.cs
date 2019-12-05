using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Neodenit.Memento.Models.DataModels
{
    public class Cloze
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

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public Guid CardID { get; set; }

        [JsonIgnore]
        public virtual Card Card { get; set; }

        public virtual ICollection<UserRepetition> UserRepetitions { get; set; }

        public UserRepetition GetUserRepetition(string username) =>
            UserRepetitions.SingleOrDefault(x => x.UserName == username);

        public IEnumerable<string> GetUsers() =>
            UserRepetitions.Select(x => x.UserName).Distinct();

        public string Label { get; set; }
    }
}
