namespace Memento.Interfaces
{
    public interface IDeck
    {
        int StartDelay { get; set; }

        double Coeff { get; set; }

        bool AllowSmallDelays { get; set; }
    }
}
