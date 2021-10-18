using Macaw.Pdf.Model;

namespace Macaw.Pdf
{
    public interface IPdfSharpService
    {
        string CreatePdf(PdfData pdfData);
    }
}