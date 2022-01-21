using System.Collections.Generic;

namespace Macaw.Pdf.Documents.Thurlede.Model
{
    public class ContributieFactuur : ThurledeFactuur
    {
        public IEnumerable<InvoiceLine> InvoiceLines { get; set; }
    }
}