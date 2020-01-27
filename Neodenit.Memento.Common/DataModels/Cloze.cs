using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Neodenit.Memento.Common.DataModels
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

        public string Label { get; set; }
    }
}
