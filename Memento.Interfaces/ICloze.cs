using System;
using System.Collections.Generic;

namespace Memento.Interfaces
{
    public interface ICloze
    {
        Guid CardID { get; set; }
        int ID { get; set; }
        string Label { get; set; }

        ICard GetCard();
        IUserRepetition GetUserRepetition(string username);
        IEnumerable<string> GetUsers();
    }
}