using System;
using System.ComponentModel.DataAnnotations;

namespace Neodenit.Memento.Common.ViewModels
{
    public class ViewCardViewModel
    {
        public Guid ID { get; set; }

        public Guid DeckID { get; set; }

        [DataType(DataType.MultilineText)]
        public string Text { get; set; }

        public string DeckTitle { get; set; }
    }
}
