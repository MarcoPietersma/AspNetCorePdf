using Macaw.Pdf.Model;

namespace Macaw.Pdf.Documents.Thurlede.Model
{
    public class WerkbeurtFactuur : ThurledeFactuur, IPdfData
    {
        public int AantalWerkbeurtenGemist { get; set; }
        public string InvoiceBodyText { get; set; }
        public decimal PrijsPerWerkbeurt { get; set; }
    }
}