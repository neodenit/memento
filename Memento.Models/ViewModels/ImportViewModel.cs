using Memento.Common.App_GlobalResources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.ViewModels
{
    public class ImportViewModel
    {
        public int DeckID { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "IsShuffled")]
        public bool IsShuffled { get; set; }
    }
}
