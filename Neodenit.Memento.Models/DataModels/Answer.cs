using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neodenit.Memento.Models.DataModels
{
    public class Answer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public DateTime Time { get; set; }

        public string Owner { get; set; }

        public bool IsCorrect { get; set; }

        public Guid ClozeID { get; set; }

        public Guid CardID { get; set; }

        public Guid DeckID { get; set; }
    }
}
