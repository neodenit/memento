using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Memento.Models.Models
{
    public class UserRepetition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public string UserName { get; set; }

        public Guid ClozeID { get; set; }

        public virtual Cloze Cloze { get; set; }

        public Cloze GetCloze()
        {
            return Cloze;
        }

        public int Position { get; set; }

        public bool IsNew { get; set; }

        public int LastDelay { get; set; }
    }
}
