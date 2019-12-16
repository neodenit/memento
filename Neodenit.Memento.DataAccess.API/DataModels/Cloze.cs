using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Neodenit.Memento.DataAccess.API.DataModels
{
    public class Cloze
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public Guid CardID { get; set; }

        [JsonIgnore]
        public virtual Card Card { get; set; }

        public virtual ICollection<UserRepetition> UserRepetitions { get; set; } = new List<UserRepetition>();

        public UserRepetition GetUserRepetition(string username) =>
            UserRepetitions.SingleOrDefault(x => x.UserName == username);

        public IEnumerable<string> GetUsers() =>
            UserRepetitions.Select(x => x.UserName).Distinct();

        public string Label { get; set; }
    }
}
