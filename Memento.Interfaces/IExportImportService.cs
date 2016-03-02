using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IExportImportService
    {
        Task<string> Export(int deckID);
        Task Import(string text, int deckID);
    }
}