using System;
using System.ComponentModel.DataAnnotations;
using Neodenit.Memento.Common.App_GlobalResources;

namespace Neodenit.Memento.Common.ViewModels
{
    public class ImportViewModel
    {
        public Guid DeckID { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "IsShuffled")]
        public bool IsShuffled { get; set; }
    }
}
