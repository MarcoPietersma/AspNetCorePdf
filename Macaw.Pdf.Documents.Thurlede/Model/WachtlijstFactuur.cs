using Macaw.Pdf.Model;
using System;

namespace Macaw.Pdf.Documents.Thurlede.Model
{
    public class WachtlijstFactuur : IPdfData
    {
        public string Address { get; set; }
        public decimal Amount { get; set; }
        public string City { get; set; }
        public string DocumentName { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Name { get; set; }
        public string PaymentReference { get; set; }
        public string PostCode { get; set; }
    }
}