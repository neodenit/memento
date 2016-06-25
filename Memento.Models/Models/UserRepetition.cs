using Memento.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Models.Models
{
    public class UserRepetition : IUserRepetition
    {
        public int ID { get; set; }

        public string UserName { get; set; }

        public int ClozeID { get; set; }

        public virtual Cloze Cloze { get; set; }

        public int Position { get; set; }

        public bool IsNew { get; set; }

        public int LastDelay { get; set; }
    }
}
