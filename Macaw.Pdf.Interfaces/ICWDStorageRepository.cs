using System.IO;
using System.Threading.Tasks;

namespace Macaw.Pdf.Interfaces
{
    public interface ICWDStorageRepository
    {
        Task<Stream> GetFileFromStorage(string reference, string extension);

        Task<string> ReadTextFromFile(string filename);

        Task WriteFileToStorage(string reference, string extension, Stream fileSteam);
    }
}