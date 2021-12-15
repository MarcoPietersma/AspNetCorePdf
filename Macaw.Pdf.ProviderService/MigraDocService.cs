using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Macaw.Pdf;

public abstract class MigraDocService<T> : IMigraDocService<T> where T : IPdfData
{
    public readonly string _createdDocsPath = String.Empty;

    protected MigraDocService()
    {
        _createdDocsPath = Path.Combine(Path.GetTempPath(), "PdfProvider\\Created");

        var currentDirectory = new DirectoryInfo(Path.GetTempPath());
        currentDirectory.CreateSubdirectory("PdfProvider\\Created");
    }

    public string FontDirectory { get; set; } = ".\\Resources";

    public abstract Task<Document> CreateDocument(T pdfData);

    public async Task<string> CreateMigraDocPdf(T pdfData)
    {
        // Create a MigraDoc document
        GlobalFontSettings.FontResolver = new FontResolver(FontDirectory);
        var document = await CreateDocument(pdfData);
        var mdddlName = $"{_createdDocsPath}/{pdfData.DocumentName}-{DateTime.UtcNow.ToOADate()}.mdddl";
        var docName = $"{_createdDocsPath}/{pdfData.DocumentName}-{DateTime.UtcNow.ToOADate()}.pdf";

        MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile(document, mdddlName);

        var renderer = new PdfDocumentRenderer(true)
        {
            Document = document
        };
        renderer.RenderDocument();
        renderer.PdfDocument.Save(docName);

        return docName;
    }
}