using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;

namespace Macaw.Pdf
{
    public interface IMigraDocService<T> where T : IPdfData
    {
        Document CreateDocument(T pdfData);

        string CreateMigraDocPdf(T pdfData);
    }
}