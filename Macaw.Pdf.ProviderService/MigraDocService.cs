using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.IO;

namespace Macaw.Pdf
{
    public abstract class MigraDocService<T> : IMigraDocService<T> where T : IPdfData
    {
        public readonly string _createdDocsPath = ".\\PdfProvider\\Created";

        protected MigraDocService()
        {
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            currentDirectory.CreateSubdirectory(_createdDocsPath);
        }

        public abstract Document CreateDocument(T pdfData);

        public string CreateMigraDocPdf(T pdfData)
        {
            // Create a MigraDoc document
            var document = CreateDocument(pdfData);
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
}