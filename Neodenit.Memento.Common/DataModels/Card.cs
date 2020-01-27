using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Neodenit.Memento.Common.DataModels
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

        public bool IsValid { get; set; }

        public bool IsDeleted { get; set; }
    }
}
