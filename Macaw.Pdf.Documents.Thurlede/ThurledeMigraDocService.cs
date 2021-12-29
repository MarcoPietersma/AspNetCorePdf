using Macaw.Pdf.Documents.Thurlede.Model;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    public class ThurledeMigraDocService<T> : MigraDocService<T> where T : IPdfData
    {
        private readonly IThurledeStorageRepository ThurledeStorageRepository;

        public ThurledeMigraDocService(IThurledeStorageRepository ThurledeStorageRepository) : base()
        {
            this.ThurledeStorageRepository = ThurledeStorageRepository;
        }

        public override async Task<Document> CreateDocument(T pdfData)
        {
            var data = pdfData as WachtlijstFactuur;

            var constructor = new WachtLijstFactuurDocumentConstructor(data, ThurledeStorageRepository);

            return await constructor.Create();
        }
    }
}