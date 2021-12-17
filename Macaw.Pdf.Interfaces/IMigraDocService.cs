using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    public interface IMigraDocService<T> where T : IPdfData
    {
        string FontDirectory
        {
            get; set;
        }

        void Clean(T pdfData);

        Task<Document> CreateDocument(T pdfData);

        Task<string> CreateMigraDocPdf(T pdfData);
    }
}