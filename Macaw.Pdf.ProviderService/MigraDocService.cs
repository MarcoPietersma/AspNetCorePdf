using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Macaw.Pdf;

public abstract class MigraDocService<T> : IMigraDocService<T> where T : IPdfData
{
    public readonly string _createdDocsPath = ".\\PdfProvider\\Created";

    protected MigraDocService()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        currentDirectory.CreateSubdirectory(_createdDocsPath);
    }

    public abstract Task<Document> CreateDocument(T pdfData);

    public async Task<string> CreateMigraDocPdf(T pdfData)
    {
        // Create a MigraDoc document
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