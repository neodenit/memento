using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Memento.Interfaces
{
    public interface IExportImportService
    {
        Task<string> Export(Guid deckID);

        Task Import(string deckText, Guid deckID);

        Task<IEnumerable<string>> ConvertApkg(Stream inputStream);
    }
}