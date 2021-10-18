using Macaw.Pdf.Model;

namespace Macaw.Pdf
{
    public interface IMigraDocService
    {
        string CreateMigraDocPdf(PdfData pdfData);
    }
}