﻿namespace Memento.Core
{
    public interface ICard
    {
        int Position { get; set; }

        int LastDelay { get; set; }

        bool IsNew { get; set; }

        int CardID { get; }
    }
}
