using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.ViewModels
{
    public class DeckViewModel
    {
        public int ID { get; set; }

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

        public DeckViewModel() { }

        public DeckViewModel(IDeck deck)
        {
            ID = deck.ID;
            Title = deck.Title;
            ControlMode = deck.ControlMode;
            DelayMode = deck.DelayMode;
            StartDelay = deck.StartDelay;
            Coeff = deck.Coeff;
            FirstDelay = deck.StartDelay;
            SecondDelay = (int)Math.Round(deck.StartDelay * deck.Coeff);
            PreviewAnswer = deck.PreviewAnswer;
        }
    }
}
