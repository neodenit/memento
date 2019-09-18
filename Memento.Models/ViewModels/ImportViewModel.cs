using System;
using System.ComponentModel.DataAnnotations;
using Memento.Common.App_GlobalResources;

namespace Memento.Models.ViewModels
{
    public class ImportViewModel
    {
        public Guid DeckID { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "IsShuffled")]
        public bool IsShuffled { get; set; }
    }
}
