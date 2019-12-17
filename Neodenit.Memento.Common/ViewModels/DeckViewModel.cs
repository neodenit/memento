using System;
using System.ComponentModel.DataAnnotations;
using Neodenit.Memento.Common.Enums;

namespace Neodenit.Memento.Common.ViewModels
{
    public class DeckViewModel
    {
        public Guid ID { get; set; }

        public string Title { get; set; }

        [Display(Name = "Control Mode")]
        public ControlModes ControlMode { get; set; }

        [Display(Name = "Delay Mode")]
        public DelayModes DelayMode { get; set; }

        [Display(Name = "Initial Delay")]
        public int StartDelay { get; set; }

        [Display(Name = "Coefficient")]
        public double Coeff { get; set; }

        [Display(Name = "First Delay")]
        public int FirstDelay { get; set; }

        [Display(Name = "Second Delay")]
        public int SecondDelay { get; set; }

        [Display(Name = "Preview Answer")]
        public bool PreviewAnswer { get; set; } = true;

        public int CardsCount { get; set; }

        public int ValidCardsCount { get; set; }

        public string Owner { get; set; }
    }
}
