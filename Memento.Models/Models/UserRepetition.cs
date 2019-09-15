namespace Memento.Models.Models
{
    public class UserRepetition
    {
        public int ID { get; set; }

        public string UserName { get; set; }

        public int ClozeID { get; set; }

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
