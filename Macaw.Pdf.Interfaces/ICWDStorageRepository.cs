using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Macaw.Pdf.Interfaces
{
    public interface ICWDStorageRepository
    {
        Task<(Stream Stream, string MimeType)> GetFileFromStorage(string reference);

        Task<string> ReadTextFromFile(string filename);


        Task WriteFileToStorage(string reference, string extension, Stream fileSteam);
    }
}