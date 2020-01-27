using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Neodenit.Memento.Common.Enums;

namespace Neodenit.Memento.Common.DataModels
{
    [Serializable]
    public class Deck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ID { get; set; }

        public bool IsShared { get; set; }

        [Required]
        public string Title { get; set; }

        public string Owner { get; set; }

        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();

        [Display(Name = "Control Mode")]
        public ControlModes ControlMode { get; set; }

        [Display(Name = "Delay Mode")]
        public DelayModes DelayMode { get; set; }

        [Display(Name = "Allow Small Delays")]
        public bool AllowSmallDelays { get; set; }

        [Display(Name = "Initial Delay")]
        public int StartDelay { get; set; }

        [Display(Name = "Coefficient")]
        public double Coeff { get; set; }

        public bool PreviewAnswer { get; set; }

        [NotMapped]
        public IEnumerable<Card> ValidCards =>
            Cards.Where(card => card.IsValid && !card.IsDeleted);

        [NotMapped]
        public IEnumerable<Card> DraftCards =>
            Cards.Where(card => !card.IsValid && !card.IsDeleted);

        [NotMapped]
        public IEnumerable<Card> DeletedCards =>
            Cards.Where(card => card.IsDeleted);

    }
}
